using System.Net;
using System.Text.Json;
using Contract.Identity.UserManager;
using Contract.Identity.WebAuthn;
using Core.Exceptions;
using Domain.Identity.UserSecurityKeys;
using Domain.Identity.Users;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlServ4r.EntityFramework;
using Volo.Abp.DependencyInjection;

namespace Application.Identity.WebAuthn
{
    /// <summary>
    /// The four WebAuthn ceremonies: register (signed in), enroll (first key, not signed in yet),
    /// and login.
    /// </summary>
    public partial class WebAuthnService : IWebAuthnService, ITransientDependency
    {
        private readonly IFido2 _fido2;
        private readonly DreamContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserManagerService _userManagerService;
        private readonly IWebAuthnSettingService _settingService;
        private readonly WebAuthnCeremonyStore _ceremonyStore;
        private readonly WebAuthnTokenValidator _tokenValidator;
        private readonly WebAuthnKeyNotifier _notifier;
        private readonly ILogger<WebAuthnService> _logger;

        public WebAuthnService(
            IFido2 fido2,
            DreamContext context,
            UserManager<User> userManager,
            IUserManagerService userManagerService,
            IWebAuthnSettingService settingService,
            WebAuthnCeremonyStore ceremonyStore,
            WebAuthnTokenValidator tokenValidator,
            WebAuthnKeyNotifier notifier,
            ILogger<WebAuthnService> logger)
        {
            _fido2 = fido2;
            _context = context;
            _userManager = userManager;
            _userManagerService = userManagerService;
            _settingService = settingService;
            _ceremonyStore = ceremonyStore;
            _tokenValidator = tokenValidator;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task<WebAuthnFeatureStatusDto> GetFeatureStatusAsync()
        {
            var settings = await _settingService.GetAppliedAsync();
            return new WebAuthnFeatureStatusDto { Enabled = settings.IsEnabled, Enforced = settings.IsEnforced };
        }

        // ---------- registration (already signed in) ----------

        public async Task<WebAuthnCeremonyOptionsDto> BeginRegistrationAsync(int userId)
        {
            var user = await GetActiveUserAsync(userId);
            return await BuildRegistrationOptionsAsync(user, CeremonyId.ForUser(userId));
        }

        public async Task<SecurityKeyEnrollResultDto> CompleteRegistrationAsync(int userId, WebAuthnRegisterCompleteDto input)
        {
            var user = await GetActiveUserAsync(userId);
            var key = await StoreNewCredentialAsync(user, CeremonyId.ForUser(userId), input);
            return new SecurityKeyEnrollResultDto { Key = key };
        }

        // ---------- enrollment (no session yet) ----------

        public async Task<WebAuthnCeremonyOptionsDto> BeginEnrollmentAsync(string enrollToken)
        {
            var identity = _tokenValidator.Validate(enrollToken, WebAuthnTokenTypes.Enrollment);
            var user = await GetActiveUserAsync(identity.UserId);
            await EnsureEnrollmentStillOwedAsync(user.Id);

            return await BuildRegistrationOptionsAsync(user, CeremonyId.ForJti(identity.Jti));
        }

        public async Task<SecurityKeyEnrollResultDto> CompleteEnrollmentAsync(WebAuthnEnrollCompleteDto input)
        {
            var identity = _tokenValidator.Validate(input.EnrollToken, WebAuthnTokenTypes.Enrollment);
            var user = await GetActiveUserAsync(identity.UserId);

            // Replay defence without any stored "used" marker: the token is only good while the
            // account still has no key, so finishing enrollment invalidates it by construction.
            // A marker in process memory would not survive the container restarts that happen on
            // every deploy — and this token can mint a session.
            await EnsureEnrollmentStillOwedAsync(user.Id);

            var key = await StoreNewCredentialAsync(user, CeremonyId.ForJti(identity.Jti), input);

            return new SecurityKeyEnrollResultDto
            {
                Key = key,
                // Note: no UserModel. The device/provider context belonged to the sign-in request
                // and is not carried through the ceremony, so CreateUserToken records its defaults
                // ("Website"/"AccessToken") — same as the TOTP path already does. Inventing a new
                // provider string here would fragment user_tokens rather than restore anything.
                Token = await _userManagerService.IssueTokenForUserAsync(
                    user.Id, proof: SecondFactorProof.SecurityKey)
            };
        }

        // ---------- login ----------

        public async Task<WebAuthnCeremonyOptionsDto> BeginLoginAsync(string temporaryToken)
        {
            var identity = _tokenValidator.Validate(temporaryToken, WebAuthnTokenTypes.SecurityKeyStep);
            EnsureNotLockedOut(identity.UserId);

            var credentials = await _context.UserSecurityKeys
                .AsNoTracking()
                .Where(x => x.UserId == identity.UserId)
                .Select(x => x.CredentialId)
                .ToListAsync();

            if (credentials.Count == 0)
            {
                throw new GlobalException("Tài khoản chưa đăng ký khoá bảo mật.", HttpStatusCode.BadRequest);
            }

            var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                AllowedCredentials = credentials
                    .Select(id => new PublicKeyCredentialDescriptor(id))
                    .ToList(),
                UserVerification = UserVerificationRequirement.Preferred
            });

            return StoreOptions(CeremonyId.ForJti(identity.Jti), options.ToJson());
        }

        public async Task<TokenDto> CompleteLoginAsync(WebAuthnLoginCompleteDto input)
        {
            var identity = _tokenValidator.Validate(input.TemporaryToken, WebAuthnTokenTypes.SecurityKeyStep);
            EnsureNotLockedOut(identity.UserId);

            var optionsJson = _ceremonyStore.TakeChallenge(CeremonyId.ForJti(identity.Jti))
                ?? throw new GlobalException("Phiên xác thực đã hết hạn. Hãy thử lại.", HttpStatusCode.BadRequest);

            var response = Deserialize<AuthenticatorAssertionRawResponse>(input.AssertionJson);
            var credentialId = DecodeCredentialId(response.Id);

            // The credential is looked up BY (user in the token, credential id) — never by
            // credential id alone. Verifying a signature and then issuing a session for whoever
            // the token names would let an attacker with a stolen password sign in to the victim
            // using their own key.
            var stored = await _context.UserSecurityKeys
                .FirstOrDefaultAsync(x => x.UserId == identity.UserId && x.CredentialId == credentialId);

            if (stored == null)
            {
                _ceremonyStore.RecordFailure(identity.UserId);
                _logger.LogWarning("WebAuthn assertion for user {UserId} used a credential that is not theirs.", identity.UserId);
                throw new GlobalException("Khoá bảo mật không hợp lệ.", HttpStatusCode.Unauthorized);
            }

            VerifyAssertionResult result;
            try
            {
                result = await _fido2.MakeAssertionAsync(new MakeAssertionParams
                {
                    AssertionResponse = response,
                    OriginalOptions = AssertionOptions.FromJson(optionsJson),
                    StoredPublicKey = stored.PublicKey,
                    StoredSignatureCounter = stored.SignatureCounter,
                    IsUserHandleOwnerOfCredentialIdCallback = (_, _) => Task.FromResult(true)
                });
            }
            catch (Fido2VerificationException ex)
            {
                _ceremonyStore.RecordFailure(identity.UserId);
                _logger.LogWarning(ex, "WebAuthn assertion failed for user {UserId}.", identity.UserId);
                throw new GlobalException("Khoá bảo mật không hợp lệ.", HttpStatusCode.Unauthorized);
            }

            // A counter that does not advance is the documented signature of a cloned key.
            if (stored.SignatureCounter > 0 && result.SignCount <= stored.SignatureCounter)
            {
                _ceremonyStore.RecordFailure(identity.UserId);
                _logger.LogWarning(
                    "WebAuthn signature counter did not advance for user {UserId} (stored {Stored}, presented {Presented}).",
                    identity.UserId, stored.SignatureCounter, result.SignCount);
                throw new GlobalException("Khoá bảo mật không hợp lệ.", HttpStatusCode.Unauthorized);
            }

            stored.SignatureCounter = result.SignCount;
            stored.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _ceremonyStore.ClearFailures(identity.UserId);

            return await _userManagerService.IssueTokenForUserAsync(
                identity.UserId, proof: SecondFactorProof.SecurityKey);
        }

        private static byte[] DecodeCredentialId(string base64Url)
        {
            try
            {
                return System.Buffers.Text.Base64Url.DecodeFromChars(base64Url);
            }
            catch (FormatException)
            {
                throw new GlobalException("Dữ liệu khoá bảo mật không đọc được.", HttpStatusCode.BadRequest);
            }
        }

        private static T Deserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json)
                    ?? throw new GlobalException("Dữ liệu khoá bảo mật không đọc được.", HttpStatusCode.BadRequest);
            }
            catch (JsonException)
            {
                throw new GlobalException("Dữ liệu khoá bảo mật không đọc được.", HttpStatusCode.BadRequest);
            }
        }

        private void EnsureNotLockedOut(int userId)
        {
            if (_ceremonyStore.IsLockedOut(userId))
            {
                throw new GlobalException(
                    "Đã thử khoá bảo mật quá nhiều lần. Vui lòng đợi ít phút.", HttpStatusCode.TooManyRequests);
            }
        }

        private static class CeremonyId
        {
            // Registration from the profile page keys by user, so two tabs starting a ceremony at
            // once overwrite each other's challenge and the second fails with "phiên đăng ký đã
            // hết hạn". Rare and self-explanatory; login and enroll key by the token jti instead.
            public static string ForUser(int userId) => $"user:{userId}";
            public static string ForJti(string jti) => $"jti:{jti}";
        }
    }
}
