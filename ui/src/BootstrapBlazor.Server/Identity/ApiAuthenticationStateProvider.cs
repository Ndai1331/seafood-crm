using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using BootstrapBlazor.Server.Http;
namespace BootstrapBlazor.Server.Identity
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        // Keys backing the "remember me" session-flag mechanism.
        // When the user does not tick "remember me", the token still lives in
        // localStorage but is treated as session-scoped: a sessionStorage marker
        // is set at login and cleared automatically by the browser when the last
        // tab closes. On the next cold start the missing marker signals that the
        // session expired, so the token is purged before use.
        private const string RememberFlagKey = "remember-me";
        private const string SessionMarkerKey = "session-active";

        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        private readonly SemaphoreSlim _stateLock = new(1, 1);
        private CancellationTokenSource? _logoutScheduleCts;
        private Task<AuthenticationState>? _authenticationStateTask = Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))
        );

        public ApiAuthenticationStateProvider(
            ILocalStorageService localStorage,
            IJSRuntime jsRuntime)
        {
            _localStorage = localStorage;
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            await _stateLock.WaitAsync();
            try
            {
                InjectServiceForHttpClient();
                var savedToken = await _localStorage.GetItemAsync<string>("my-access-token");
                System.Console.WriteLine($"[AuthFlow] AuthProvider.GetAuthenticationState localStorage {RequestClient.DescribeToken(savedToken)}");

                if (string.IsNullOrWhiteSpace(savedToken))
                {
                    System.Console.WriteLine("[AuthFlow] AuthProvider.GetAuthenticationState -> anonymous: token missing");
                    CancelAutoLogoutSchedule();
                    var anonymousState = CreateAnonymousState();
                    _authenticationStateTask = Task.FromResult(anonymousState);
                    return anonymousState;
                }

                // "Remember me" enforcement: if the login was not marked persistent
                // and the browser-session marker is gone (browser fully closed and
                // reopened), the session-scoped login has expired — purge the token.
                if (await IsSessionScopedLoginExpiredAsync())
                {
                    await ClearTokensAsync();
                    CancelAutoLogoutSchedule();
                    var anonymousState = CreateAnonymousState();
                    _authenticationStateTask = Task.FromResult(anonymousState);
                    return anonymousState;
                }

                RequestClient.AttachToken(savedToken);
                var claims = ParseClaimsFromJwt(savedToken);

                if (!TryGetTokenExpirationUtc(claims, out var expiresAtUtc) || expiresAtUtc <= DateTime.UtcNow)
                {
                    System.Console.WriteLine($"[AuthFlow] AuthProvider.GetAuthenticationState -> anonymous: token expired/invalid exp:{expiresAtUtc:yyyy-MM-dd HH:mm:ss}");
                    await ClearTokensAsync();
                    CancelAutoLogoutSchedule();
                    var anonymousState = CreateAnonymousState();
                    _authenticationStateTask = Task.FromResult(anonymousState);
                    NotifyAuthenticationStateChanged(_authenticationStateTask);
                    return anonymousState;
                }

                ScheduleAutoLogout(expiresAtUtc);
                System.Console.WriteLine($"[AuthFlow] AuthProvider.GetAuthenticationState -> authenticated exp:{expiresAtUtc:yyyy-MM-dd HH:mm:ss}");

                var authenticatedState = new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"))
                );
                _authenticationStateTask = Task.FromResult(authenticatedState);
                NotifyAuthenticationStateChanged(_authenticationStateTask);
                return authenticatedState;
            }
            catch
            {
                System.Console.WriteLine("[AuthFlow] AuthProvider.GetAuthenticationState -> anonymous: exception while reading token");
                CancelAutoLogoutSchedule();
                var anonymousState = CreateAnonymousState();
                _authenticationStateTask = Task.FromResult(anonymousState);
                return anonymousState;
            }
            finally
            {
                _stateLock.Release();
            }
        }

        public void SetAuthenticationState(Task<AuthenticationState> authenticationStateTask)
        {
            _authenticationStateTask = authenticationStateTask ?? throw new ArgumentNullException(nameof(authenticationStateTask));
            NotifyAuthenticationStateChanged(_authenticationStateTask);
        }

        public void Logout()
        {
            _ = MarkUserAsLoggedOut();
        }

        public bool CheckExpiredToken(IEnumerable<Claim> claims)
        {
            return TryGetTokenExpirationUtc(claims, out var expiresAtUtc) && expiresAtUtc > DateTime.UtcNow;
        }

        private void InjectServiceForHttpClient()
        {
            RequestClient.InjectServices(_localStorage);
        }

        public async Task MarkUserAsAuthenticated(string userName)
        {
            System.Console.WriteLine($"[AuthFlow] AuthProvider.MarkUserAsAuthenticated user:{userName}");
            await GetAuthenticationStateAsync();
        }

        public async Task MarkUserAsLoggedOut()
        {
            System.Console.WriteLine("[AuthFlow] AuthProvider.MarkUserAsLoggedOut clearing tokens");
            CancelAutoLogoutSchedule();
            await ClearTokensAsync();
            var authState = Task.FromResult(CreateAnonymousState());
            _authenticationStateTask = authState;
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task<string> GetCurrentUserId()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("my-access-token");
            if (string.IsNullOrEmpty(savedToken))
            {
                return null;
            }
            var claims = ParseClaimsFromJwt1(savedToken);
            var userId = claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid)?.Value;
            return userId;
        }


        public async Task<string> GetUserRolesAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("my-access-token");
            if (string.IsNullOrEmpty(savedToken))
            {
                return null;
            }
            var claims = ParseClaimsFromJwt1(savedToken);
            var roleClaims = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            return roleClaims?.Value;
        }

        /// <summary>
        /// ALL role claims from the JWT. Users can hold multiple roles
        /// (e.g. ADMIN + SUPER_ADMIN) — GetUserRolesAsync only returns the first,
        /// so any role-membership check MUST use this list instead.
        /// </summary>
        public async Task<List<string>> GetUserRoleListAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("my-access-token");
            if (string.IsNullOrEmpty(savedToken))
            {
                return new List<string>();
            }
            return ParseClaimsFromJwt1(savedToken)
                .Where(x => x.Type == ClaimTypes.Role)
                .Select(x => x.Value)
                .ToList();
        }

        /// <summary>True when the current JWT holds the SUPER_ADMIN role.</summary>
        public async Task<bool> IsSuperAdminAsync()
        {
            var roles = await GetUserRoleListAsync();
            return roles.Contains("SUPER_ADMIN");
        }

        /// <summary>Impersonated username when the current token is an impersonation session, else null.</summary>
        public async Task<string?> GetImpersonatedUserNameAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("my-access-token");
            if (string.IsNullOrEmpty(savedToken))
            {
                return null;
            }
            var claims = ParseClaimsFromJwt1(savedToken).ToList();
            var isImpersonating = claims.Any(x => x.Type == "impersonator_id");
            if (!isImpersonating)
            {
                return null;
            }
            return claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value ?? "user";
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            return ParseClaimsFromJwt1(jwt);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt1(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return Enumerable.Empty<Claim>();
            }
            
            var handler = new JwtSecurityTokenHandler();

            var decodedValue = handler.ReadJwtToken(jwt);
            return decodedValue.Claims;
        }

        private static bool TryGetTokenExpirationUtc(IEnumerable<Claim> claims, out DateTime expiresAtUtc)
        {
            expiresAtUtc = DateTime.MinValue;
            var expiredClaim = claims.FirstOrDefault(x => x.Type == "exp")?.Value;
            if (!long.TryParse(expiredClaim, out var epochTime))
            {
                return false;
            }

            expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(epochTime).UtcDateTime;
            return true;
        }

        private void ScheduleAutoLogout(DateTime expiresAtUtc)
        {
            CancelAutoLogoutSchedule();
            var delay = expiresAtUtc - DateTime.UtcNow;
            if (delay <= TimeSpan.Zero)
            {
                _ = MarkUserAsLoggedOut();
                return;
            }

            var cts = new CancellationTokenSource();
            _logoutScheduleCts = cts;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay, cts.Token);
                    if (!cts.IsCancellationRequested)
                    {
                        await MarkUserAsLoggedOut();
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignore cancellation from re-scheduling or logout.
                }
            });
        }

        private void CancelAutoLogoutSchedule()
        {
            if (_logoutScheduleCts == null)
            {
                return;
            }

            _logoutScheduleCts.Cancel();
            _logoutScheduleCts.Dispose();
            _logoutScheduleCts = null;
        }

        private Task ClearTokensAsync() => RequestClient.RunTokenMutationAsync(async () =>
        {
            await _localStorage.RemoveItemAsync("my-access-token");
            await _localStorage.RemoveItemAsync("my-refresh-token");
            await _localStorage.RemoveItemAsync(RememberFlagKey);
        });

        /// <summary>
        /// True when the current login was NOT persistent ("remember me" off) and the
        /// per-browser-session marker is missing — i.e. the browser was fully closed
        /// and reopened, so a non-persistent session must not survive.
        /// Any failure (e.g. JS unavailable during prerender) is treated as "not expired"
        /// so the check never blocks a legitimate persistent login.
        /// </summary>
        private async Task<bool> IsSessionScopedLoginExpiredAsync()
        {
            try
            {
                // Absent flag = a session that predates this feature (or a persistent
                // SSO login). Treat as persistent so existing logins are never purged.
                if (!await _localStorage.ContainKeyAsync(RememberFlagKey))
                {
                    return false;
                }

                var remember = await _localStorage.GetItemAsync<bool>(RememberFlagKey);
                if (remember)
                {
                    return false;
                }

                var marker = await _jsRuntime.InvokeAsync<string?>(
                    "sessionStorage.getItem", SessionMarkerKey);
                return string.IsNullOrEmpty(marker);
            }
            catch
            {
                return false;
            }
        }

        private static AuthenticationState CreateAnonymousState()
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void Dispose()
        {
            CancelAutoLogoutSchedule();
            _stateLock.Dispose();
        }
    }
}
