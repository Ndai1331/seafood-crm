using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Identity;
using BootstrapBlazor.Server.Data;

namespace BootstrapBlazor.Server.Services
{
    public interface IUserManagerService
    {
        Task<List<UserWithNavigationPropertiesDto>> GetListWithNavigationAsync();
        Task<List<UserBasicInfoDto>> GetBasicInfoUsersWithNavigationAsync(FilterBaseData filter);
        Task<UserWithNavigationPropertiesDto> GetWithNavigationProperties(int id);
        Task<List<UserIdentityDto>> GetBasicUserInfosAsync(string role = "");

        Task<List<UserDto>> GetListAsync();

        Task<List<UserIdentityDto>> GetBasicUserInfoAsync(string role = "");

        Task<UserDto> CreateUserWithNavigationPropertiesAsync(CreateUserDto input);

        Task<TokenDto?> SignInAsync(UserModel input, bool rememberMe = true);

        /// <summary>SUPER_ADMIN starts a read-only impersonation session for target user (returns true on success).</summary>
        Task<bool> StartImpersonationAsync(int userId);

        /// <summary>Restore the original SUPER_ADMIN session after impersonation.</summary>
        Task StopImpersonationAsync();

        Task<bool> SetNewPasswordAsync(NewUserPasswordDto input);

        Task<TokenDto?> RefreshTokenAsync(TokenModel token);

        void Logout();

        Task<List<UserDto>> GetListByRoles(string roleName);

        Task<List<UserDto>> GetListByRoles(int createBy, string roleName);

        Task<ApiResponseBase<List<UserDto>>> GetListByFilterAsync(UserFilterPagingModel filter);
        Task<List<UserIdentityDto>> GetBasicSeoUserInfoAsync(int? userId = null, string? role = null, UserType? userType = null, int? teamId = null);
        
        Task<UserDto> UpdateUserWithNavigationPropertiesAsync(UpdateUserDto input, int id);
        Task<UserDto> CloneUserAsync(int id);
        
        /// <summary>
        /// Lấy thông tin profile của user hiện tại
        /// </summary>
        Task<UserWithNavigationPropertiesDto> GetMyProfileAsync(int userId);
        
        /// <summary>
        /// Cập nhật thông tin cá nhân (chỉ các trường an toàn)
        /// </summary>
        Task<UserDto> UpdateMyProfileAsync(UpdateProfileDto input, int userId);
    }
    public class UserManagerService : IUserManagerService
    {
        private IUserManagerService userManagerService;
        private ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public UserManagerService(
            ILocalStorageService localStorage,
            IJSRuntime jsRuntime,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            _jsRuntime = jsRuntime;
        }


        public async Task<List<UserWithNavigationPropertiesDto>> GetListWithNavigationAsync()
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<UserWithNavigationPropertiesDto>>("user/get-list-with-nav");
        }

        public async Task<List<UserBasicInfoDto>> GetBasicInfoUsersWithNavigationAsync(FilterBaseData filter)
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.PostAPIAsync<List<UserBasicInfoDto>>("user/get-basic-info-users-with-navigation-properties", filter);
        }

        public async Task<UserWithNavigationPropertiesDto> GetWithNavigationProperties(int id)
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<UserWithNavigationPropertiesDto>($"user/get-with-nav-properties/{id}");
        }


        public async Task<List<UserIdentityDto>> GetBasicUserInfosAsync(string role = "")
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<UserIdentityDto>>($"user/get-basic-user-info?role={role}");
        }

        public async Task<List<UserDto>> GetListAsync()
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<UserDto>>("user");
        }

        public async Task<List<UserIdentityDto>> GetBasicUserInfoAsync(string role = "")
        {
            await EnsureRequestAuthAsync();
            return await RequestClient.GetAPIAsync<List<UserIdentityDto>>("user/get-basic-user-info");
        }

        public async Task<List<UserIdentityDto>> GetBasicSeoUserInfoAsync(int? userId = null, string? role = null, UserType? userType = null, int? teamId = null)
        {
            await EnsureRequestAuthAsync();
            var queryParams = new List<string>();
            if (userId.HasValue)
                queryParams.Add($"userId={userId.Value}");
            if (!string.IsNullOrEmpty(role))
                queryParams.Add($"role={role}");
            if (userType.HasValue)
                queryParams.Add($"userType={(int)userType.Value}");
            if (teamId.HasValue)
                queryParams.Add($"teamId={teamId.Value}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await RequestClient.GetAPIAsync<List<UserIdentityDto>>($"user/get-basic-seo-user-info{queryString}");
        }

        private async Task EnsureRequestAuthAsync()
        {
            RequestClient.InjectServices(_localStorage);
            var token = await _localStorage.GetItemAsync<string>("my-access-token");
            System.Console.WriteLine($"[AuthFlow] UserManagerService.EnsureRequestAuth {RequestClient.DescribeToken(token)}");
            if (!string.IsNullOrWhiteSpace(token))
            {
                RequestClient.AttachToken(token);
            }
        }


        public async Task<bool> StartImpersonationAsync(int userId)
        {
            RequestClient.InjectServices(_localStorage);
            var token = await _localStorage.GetItemAsync<string>("my-access-token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                RequestClient.AttachToken(token);
            }

            var response = await RequestClient.PostAPIAsync<TokenDto>($"user/impersonate/{userId}", new { }, notifyOk: false);
            if (response == null || string.IsNullOrEmpty(response.AccessToken))
            {
                return false;
            }

            // Preserve the current SUPER_ADMIN session so it can be restored on exit.
            var adminToken = await _localStorage.GetItemAsync<string>("my-access-token");
            var adminRefresh = await _localStorage.GetItemAsync<string>("my-refresh-token");
            await _localStorage.SetItemAsync("admin-access-token", adminToken);
            await _localStorage.SetItemAsync("admin-refresh-token", adminRefresh);

            // Swap in the read-only impersonation token (no refresh token by design).
            await RequestClient.RunTokenMutationAsync(async () =>
            {
                await _localStorage.SetItemAsync("my-access-token", response.AccessToken);
                await _localStorage.RemoveItemAsync("my-refresh-token");
                RequestClient.AttachToken(response.AccessToken);
            });
            return true;
        }

        public async Task StopImpersonationAsync()
        {
            RequestClient.InjectServices(_localStorage);
            var adminToken = await _localStorage.GetItemAsync<string>("admin-access-token");
            var adminRefresh = await _localStorage.GetItemAsync<string>("admin-refresh-token");
            if (!string.IsNullOrWhiteSpace(adminToken))
            {
                await RequestClient.RunTokenMutationAsync(async () =>
                {
                    await _localStorage.SetItemAsync("my-access-token", adminToken);
                    await _localStorage.SetItemAsync("my-refresh-token", adminRefresh);
                    RequestClient.AttachToken(adminToken);
                });
            }
            await _localStorage.RemoveItemAsync("admin-access-token");
            await _localStorage.RemoveItemAsync("admin-refresh-token");
        }

        public async Task<TokenDto?> SignInAsync(UserModel input, bool rememberMe = true)
        {
            try
            {
                var response = await RequestClient.PostAPIAsync<TokenDto>("user/sign-in", input);
                System.Console.WriteLine($"[AuthFlow] SignIn response null:{response == null} requires2FA:{response?.RequiresTwoFactor} access:{RequestClient.DescribeToken(response?.AccessToken)} refreshMissing:{string.IsNullOrWhiteSpace(response?.RefreshToken)}");

                // Check if response is valid
                if (response == null)
                {
                    System.Console.WriteLine("[AuthFlow] SignIn stopped: response is null");
                    return null;
                }

                if (response.RequiresTwoFactor && !string.IsNullOrEmpty(response.TemporaryToken))
                {
                    System.Console.WriteLine($"[AuthFlow] SignIn requires TOTP temporaryToken:{RequestClient.DescribeToken(response.TemporaryToken)}");
                    return response;
                }

                // Without these two branches the guard further down ("no access token -> null")
                // swallows the flags and the login page reports a wrong password to every user.
                if (response.RequiresSecurityKey && !string.IsNullOrEmpty(response.TemporaryToken))
                {
                    System.Console.WriteLine($"[AuthFlow] SignIn requires a security key temporaryToken:{RequestClient.DescribeToken(response.TemporaryToken)}");
                    return response;
                }

                if (response.RequiresSecurityKeyEnrollment && !string.IsNullOrEmpty(response.TemporaryToken))
                {
                    System.Console.WriteLine($"[AuthFlow] SignIn requires security key enrollment temporaryToken:{RequestClient.DescribeToken(response.TemporaryToken)}");
                    return response;
                }

                if (string.IsNullOrEmpty(response.AccessToken))
                {
                    System.Console.WriteLine("[AuthFlow] SignIn stopped: access token missing");
                    return null;
                }

                RequestClient.InjectServices(_localStorage);
                await RequestClient.RunTokenMutationAsync(async () =>
                {
                    await _localStorage.SetItemAsync("my-access-token", response.AccessToken);
                    await _localStorage.SetItemAsync("my-refresh-token", response.RefreshToken);
                    RequestClient.AttachToken(response.AccessToken);
                });

                // "Remember me" flag drives whether the login survives a browser restart.
                // When off, mark the session as active for the current browser session only;
                // the sessionStorage marker is cleared by the browser when the last tab closes,
                // and ApiAuthenticationStateProvider purges the token on the next cold start.
                await _localStorage.SetItemAsync("remember-me", rememberMe);
                if (!rememberMe)
                {
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "session-active", "1");
                }

                var savedAccessToken = await _localStorage.GetItemAsync<string>("my-access-token");
                var savedRefreshToken = await _localStorage.GetItemAsync<string>("my-refresh-token");
                System.Console.WriteLine($"[AuthFlow] SignIn saved localStorage access:{RequestClient.DescribeToken(savedAccessToken)} refreshMissing:{string.IsNullOrWhiteSpace(savedRefreshToken)}");
                await ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(input.UserName);
                System.Console.WriteLine("[AuthFlow] SignIn MarkUserAsAuthenticated completed");

                return response;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"SignIn Error: {ex.Message}");
                System.Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Rethrow rather than returning null. Null tells the login screen nothing, so it
                // fell back to "wrong account or password" for every failure — including the one
                // that says password login is switched off for this account. People with a correct
                // password retyped it until they gave up. The single caller (the login screen)
                // catches this and shows the API's own words.
                throw;
            }
        }
        public async Task<bool> SetNewPasswordAsync(NewUserPasswordDto input)
        {
            return await RequestClient.PostAPIAsync<bool>("user/set-password", input);
        }


        public async Task<TokenDto?> RefreshTokenAsync(TokenModel token)
        {
            try
            {
                // TODO: Implement refresh token logic
                var response = await RequestClient.PostAPIAsync<TokenDto>("user/refresh-token", token);

                if (response != null && !string.IsNullOrEmpty(response.AccessToken))
                {
                    await RequestClient.RunTokenMutationAsync(async () =>
                    {
                        await _localStorage.SetItemAsync("my-access-token", response.AccessToken);
                        await _localStorage.SetItemAsync("my-refresh-token", response.RefreshToken);
                        RequestClient.AttachToken(response.AccessToken);
                    });
                    return response;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"RefreshToken Error: {ex.Message}");
                return null;
            }
        }

        public async void Logout()
        {
            await ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
        }

        public async Task<List<UserDto>> GetListByRoles(string roleName)
        {
            return await RequestClient.GetAPIAsync<List<UserDto>>($"user/get-with-roles/{roleName}");
        }

        public async Task<List<UserDto>> GetListByRoles(int createBy, string roleName)
        {
            return await RequestClient.GetAPIAsync<List<UserDto>>($"user/get-with-roles/{createBy}/{roleName}");
        }

        public async Task<ApiResponseBase<List<UserDto>>> GetListByFilterAsync(UserFilterPagingModel filter)
        {
            return await RequestClient.PostAPIAsync<ApiResponseBase<List<UserDto>>>($"user/get-with-filters", filter);
        }

        public async Task<UserDto> CreateUserWithNavigationPropertiesAsync(CreateUserDto input)
        {
            return await RequestClient.PostAPIAsync<UserDto>("user/create-user-with-roles", input);
        }
        
        public async Task<UserDto> UpdateUserWithNavigationPropertiesAsync(UpdateUserDto input, int id)
        {
            return await RequestClient.PostAPIAsync<UserDto>($"user/update-user-with-roles/{id}", input);
        }


        public async Task<UserDto> CloneUserAsync(int id)
        {
            return await RequestClient.PostAPIAsync<UserDto>($"user/clone-user", new CloneUserDto { Id = id });
        }

        /// <summary>
        /// Lấy thông tin profile của user hiện tại
        /// </summary>
        public async Task<UserWithNavigationPropertiesDto> GetMyProfileAsync(int userId)
        {
            return await GetWithNavigationProperties(userId);
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân — chỉ map các trường an toàn từ UpdateProfileDto.
        /// Roles, Position, Team, UserCode, UserType, IsActive giữ nguyên giá trị cũ.
        /// </summary>
        public async Task<UserDto> UpdateMyProfileAsync(UpdateProfileDto input, int userId)
        {
            // 1. Lấy data hiện tại để preserve admin-only fields
            var currentUser = await GetWithNavigationProperties(userId);
            if (currentUser?.User == null)
            {
                throw new Exception("Không tìm thấy thông tin người dùng");
            }

            // 2. Tạo UpdateUserDto từ data hiện tại, chỉ override các trường an toàn
            var updateDto = new UpdateUserDto
            {
                // Preserve admin-only fields
                UserName = currentUser.User.UserName,
                UserCode = currentUser.User.UserCode,
                UserType = (int)currentUser.User.UserType,
                IsActive = currentUser.User.IsActive,
                Roles = currentUser.RoleNames,
                PositionId = currentUser.Position?.Id,
                TeamId = currentUser.Team?.Id,
                DepartmentIds = new List<int>(),
                ODX = currentUser.User.ODX ?? 0,
                CreatedBy = currentUser.User.CreatedBy,
                ModifiedBy = currentUser.User.ModifiedBy,
                CitizenIDNumber = currentUser.User.CitizenIDNumber,
                DependsId = currentUser.User.DependsId,
                Relationship = currentUser.User.Relationship,

                // Map safe fields from UpdateProfileDto
                FirstName = input.FirstName,
                LastName = input.LastName,
                Gender = input.Gender,
                DOB = input.DOB,
                PhoneNumber = input.PhoneNumber,
                Email = input.Email,
                Address = input.Address,
                AvatarURL = input.AvatarURL ?? currentUser.User.AvatarURL,
                IsSetPassword = input.IsSetPassword,
                Password = input.Password,
                PasswordConfirm = input.PasswordConfirm
            };

            // 3. Gọi API update
            return await UpdateUserWithNavigationPropertiesAsync(updateDto, userId);
        }
       
    }
}
