using System.Net;
using System.Security.Claims;
using Contract.Identity.UserManager;
using Contract.Identity.WebAuthn;
using Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    /// <summary>
    /// Security-key (WebAuthn/FIDO2) endpoints.
    ///
    /// Note the deliberate split between register/* and enroll/*. They do the same ceremony, but
    /// register/* identifies the user from the bearer token and enroll/* identifies them from a
    /// short-lived token in the body. Serving both from one endpoint would mean picking between
    /// two identities at runtime — and the browser attaches any bearer left in localStorage even
    /// on the login page, so on a shared machine that choice could bind one person's key to
    /// somebody else's account.
    /// </summary>
    [ApiController]
    [Route("api/webauthn/")]
    [Authorize]
    public class WebAuthnController : ControllerBase
    {
        private const string AdminRoles = "ADMIN,SUPER_ADMIN";

        /// <summary>For the one action that touches every account at once and cannot be undone.</summary>
        private const string SuperAdminRole = "SUPER_ADMIN";

        private readonly IWebAuthnService _webAuthnService;
        private readonly IWebAuthnSettingService _settingService;

        public WebAuthnController(IWebAuthnService webAuthnService, IWebAuthnSettingService settingService)
        {
            _webAuthnService = webAuthnService;
            _settingService = settingService;
        }

        [HttpGet("feature-status")]
        [AllowAnonymous]
        public Task<WebAuthnFeatureStatusDto> GetFeatureStatusAsync() => _webAuthnService.GetFeatureStatusAsync();

        /// <summary>
        /// Whether the login screen may show a password box for this account. Anonymous, because
        /// it is asked on the login screen before anyone is signed in; it answers with a bare
        /// yes/no and never reveals whether the account exists.
        /// </summary>
        [HttpGet("password-door")]
        [AllowAnonymous]
        public async Task<PasswordDoorDto> GetPasswordDoorAsync([FromQuery] string? userName) =>
            new PasswordDoorDto { Allowed = await _settingService.IsPasswordDoorOpenAsync(userName) };

        // ---------- signed-in registration (profile page) ----------

        [HttpPost("register/begin")]
        public Task<WebAuthnCeremonyOptionsDto> BeginRegistrationAsync() =>
            _webAuthnService.BeginRegistrationAsync(GetCurrentUserId());

        [HttpPost("register/complete")]
        public Task<SecurityKeyEnrollResultDto> CompleteRegistrationAsync(WebAuthnRegisterCompleteDto input) =>
            _webAuthnService.CompleteRegistrationAsync(GetCurrentUserId(), input);

        // ---------- first-key enrollment (no session yet) ----------

        [HttpPost("enroll/begin")]
        [AllowAnonymous]
        public Task<WebAuthnCeremonyOptionsDto> BeginEnrollmentAsync(WebAuthnCeremonyStartDto input) =>
            _webAuthnService.BeginEnrollmentAsync(input.TemporaryToken);

        [HttpPost("enroll/complete")]
        [AllowAnonymous]
        public Task<SecurityKeyEnrollResultDto> CompleteEnrollmentAsync(WebAuthnEnrollCompleteDto input) =>
            _webAuthnService.CompleteEnrollmentAsync(input);

        // ---------- login ----------

        [HttpPost("login/begin")]
        [AllowAnonymous]
        public Task<WebAuthnCeremonyOptionsDto> BeginLoginAsync(WebAuthnCeremonyStartDto input) =>
            _webAuthnService.BeginLoginAsync(input.TemporaryToken);

        [HttpPost("login/complete")]
        [AllowAnonymous]
        public Task<TokenDto> CompleteLoginAsync(WebAuthnLoginCompleteDto input) =>
            _webAuthnService.CompleteLoginAsync(input);

        // ---------- key management ----------

        [HttpGet("keys")]
        public Task<List<SecurityKeyDto>> GetKeysAsync() => _webAuthnService.GetKeysAsync(GetCurrentUserId());

        [HttpDelete("keys/{keyId}")]
        public Task DeleteKeyAsync(int keyId) => _webAuthnService.DeleteKeyAsync(GetCurrentUserId(), keyId);

        // ---------- admin ----------

        // "ADMIN,SUPER_ADMIN", not "ADMIN": SUPER_ADMIN is a separate role and would otherwise be
        // locked out of the only recovery endpoint the whole design depends on.
        [HttpGet("admin/status/{userId}")]
        [Authorize(Roles = AdminRoles)]
        public Task<AdminSecurityKeyStatusDto> AdminGetStatusAsync(int userId) =>
            _webAuthnService.AdminGetStatusAsync(userId);

        [HttpPost("admin/reset/{userId}")]
        [Authorize(Roles = AdminRoles)]
        public Task AdminResetAsync(int userId) => _webAuthnService.AdminResetAsync(GetCurrentUserId(), userId);

        // ---------- switches ----------

        [HttpGet("settings")]
        [Authorize(Roles = AdminRoles)]
        public Task<WebAuthnSettingStatusDto> GetSettingsAsync() =>
            _settingService.GetStatusAsync(GetCurrentUserId());

        [HttpPut("settings")]
        [Authorize(Roles = AdminRoles)]
        public Task<WebAuthnSettingStatusDto> UpdateSettingsAsync(WebAuthnSettingUpdateDto input) =>
            _settingService.UpdateAsync(GetCurrentUserId(), User.Identity?.Name, input);

        /// <summary>
        /// What these settings looked like after each save. Read-only and secret-free, so any
        /// admin who can see the page can see how it got into its current state.
        /// </summary>
        [HttpGet("settings/history")]
        [Authorize(Roles = AdminRoles)]
        public Task<List<WebAuthnSettingHistoryDto>> GetSettingsHistoryAsync([FromQuery] int take = 20) =>
            _settingService.GetHistoryAsync(take);

        /// <summary>
        /// Who can actually get in: every active account with its key count and its password
        /// permission. The counts alone never said which people they meant.
        /// </summary>
        [HttpGet("settings/users")]
        [Authorize(Roles = AdminRoles)]
        public Task<List<WebAuthnUserAccessDto>> GetUserAccessAsync() => _settingService.GetUserAccessAsync();

        /// <summary>
        /// Hands one account the password door, or takes it back. This is how a person without a
        /// key signs in once, while the organisation-wide switch is off, to register one.
        /// </summary>
        [HttpPut("settings/users/{userId:int}/password-login")]
        [Authorize(Roles = AdminRoles)]
        public Task<WebAuthnUserAccessDto> SetPasswordLoginAllowedAsync(
            int userId, PasswordLoginAllowedUpdateDto input) =>
            _settingService.SetPasswordLoginAllowedAsync(userId, input.Allowed, User.Identity?.Name);

        /// <summary>
        /// Puts a previous Yubico pair back. SUPER_ADMIN only: it changes what the whole company
        /// authenticates against.
        /// </summary>
        [HttpPost("settings/history/{historyId:int}/restore-yubico")]
        [Authorize(Roles = SuperAdminRole)]
        public Task<WebAuthnSettingStatusDto> RestoreYubicoAsync(int historyId) =>
            _settingService.RestoreYubicoAsync(GetCurrentUserId(), User.Identity?.Name, historyId);

        /// <summary>
        /// Wipes every registered key in the organisation. SUPER_ADMIN only, a notch above the
        /// rest of this controller: it is irreversible and it touches every account at once.
        /// </summary>
        [HttpPost("admin/reset-all-keys")]
        [Authorize(Roles = SuperAdminRole)]
        public Task<SecurityKeyResetSummaryDto> ResetAllKeysAsync() =>
            _settingService.ResetAllKeysAsync(GetCurrentUserId(), User.Identity?.Name);

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.PrimarySid)
                ?? HttpContext.User.FindFirst("primarysid")
                ?? HttpContext.User.FindFirst("sub")
                ?? HttpContext.User.FindFirst("userId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new GlobalException("Unauthorized.", HttpStatusCode.Unauthorized);
            }

            return userId;
        }
    }
}
