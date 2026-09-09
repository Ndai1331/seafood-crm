using System.Net;
using System.Security.Claims;
using Contract.Identity.UserManager;
using Contract.Identity.Yubico;
using Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    /// <summary>
    /// YubiKey (Yubico OTP) endpoints.
    ///
    /// The split between the anonymous and authorised halves matters. Anything reached with only a
    /// half-authenticated token — pin/verify and verify — takes the user's identity from that token
    /// in the body, never from a bearer header: the browser attaches whatever bearer is left in
    /// localStorage even on the login page, so on a shared machine trusting the header could bind
    /// one person's login to somebody else's account.
    /// </summary>
    [ApiController]
    [Route("api/yubikey/")]
    [Authorize]
    public class YubikeyController : ControllerBase
    {
        private const string AdminRoles = "ADMIN,SUPER_ADMIN";

        private readonly IYubikeyService _yubikeyService;

        public YubikeyController(IYubikeyService yubikeyService)
        {
            _yubikeyService = yubikeyService;
        }

        [HttpGet("feature-status")]
        [AllowAnonymous]
        public Task<YubikeyFeatureStatusDto> GetFeatureStatusAsync() => _yubikeyService.GetFeatureStatusAsync();

        // ---------- login, holding only a half-authenticated token ----------

        [HttpPost("pin/verify")]
        [AllowAnonymous]
        public Task<YubikeyPinResultDto> VerifyPinAsync(YubikeyPinDto input) =>
            _yubikeyService.VerifyPinAsync(input);

        [HttpPost("verify")]
        [AllowAnonymous]
        public Task<TokenDto> VerifyOtpAsync(YubikeyVerifyDto input) =>
            _yubikeyService.VerifyOtpAsync(input);

        /// <summary>
        /// Registering the first key without leaving the login screen. Anonymous for the same
        /// reason the two above are: the caller has cleared their password but has no session yet,
        /// and the enrollment token in the body is what names the account.
        /// </summary>
        [HttpPost("enroll/login")]
        [AllowAnonymous]
        public Task<YubikeyEnrollResultDto> EnrollAtLoginAsync(YubikeyEnrollWithTokenDto input) =>
            _yubikeyService.EnrollAtLoginAsync(input);

        /// <summary>
        /// The PIN step of that enrollment, still holding only the setup token. Anonymous for the
        /// same reason as the step before it: the session is being withheld until the account is
        /// finished, so there is no bearer to authorise with.
        /// </summary>
        [HttpPost("enroll/pin")]
        [AllowAnonymous]
        public Task<YubikeySetupProgressDto> SetPinAtLoginAsync(YubikeySetupPinDto input) =>
            _yubikeyService.SetPinAtLoginAsync(input);

        // ---------- login with no password at all ----------

        /// <summary>
        /// Anonymous by necessity: there is no token yet, and the OTP in the body is the only
        /// thing naming an account. The reply carries a token and nothing else — saying whose key
        /// it was would turn a found key into a way of learning who owns it.
        /// </summary>
        [HttpPost("login/touch")]
        [AllowAnonymous]
        public Task<YubikeyPasswordlessTouchResultDto> PasswordlessTouchAsync(
            YubikeyPasswordlessTouchDto input) => _yubikeyService.PasswordlessTouchAsync(input);

        [HttpPost("login/pin")]
        [AllowAnonymous]
        public Task<TokenDto> PasswordlessPinAsync(YubikeyPasswordlessPinDto input) =>
            _yubikeyService.PasswordlessPinAsync(input);

        // ---------- the owner, signed in ----------

        [HttpGet("keys")]
        public Task<List<YubikeyDto>> GetKeysAsync() => _yubikeyService.GetKeysAsync(GetCurrentUserId());

        [HttpPost("enroll")]
        public Task<YubikeyDto> EnrollAsync(YubikeyEnrollDto input) =>
            _yubikeyService.EnrollAsync(GetCurrentUserId(), input);

        [HttpPost("pin")]
        public Task SetPinAsync(YubikeySetPinDto input) =>
            _yubikeyService.SetPinAsync(GetCurrentUserId(), input);

        [HttpDelete("keys/{keyId:int}")]
        public Task DeleteKeyAsync(int keyId) => _yubikeyService.DeleteKeyAsync(GetCurrentUserId(), keyId);

        // ---------- administration ----------

        [HttpGet("admin/status/{userId:int}")]
        [Authorize(Roles = AdminRoles)]
        public Task<AdminYubikeyStatusDto> AdminGetStatusAsync(int userId) =>
            _yubikeyService.AdminGetStatusAsync(userId);

        [HttpPost("admin/reset/{userId:int}")]
        [Authorize(Roles = AdminRoles)]
        public Task AdminResetAsync(int userId) =>
            _yubikeyService.AdminResetAsync(GetCurrentUserId(), userId);

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
