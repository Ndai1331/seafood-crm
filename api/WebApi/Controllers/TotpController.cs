using System.Net;
using System.Security.Claims;
using Contract.Identity.Totp;
using Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/totp/")]
    [Authorize]
    public class TotpController : ControllerBase
    {
        private readonly ITotpService _totpService;
        private readonly IConfiguration _configuration;

        public TotpController(ITotpService totpService, IConfiguration configuration)
        {
            _totpService = totpService;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("feature-status")]
        [AllowAnonymous]
        public object GetFeatureStatus()
        {
            return new { featureEnabled = _configuration.GetValue<bool>("Totp:Enabled") };
        }

        // ---------- login-screen enrollment, holding only a setup token ----------
        //
        // Anonymous by necessity: the session is exactly what these steps are working towards, so
        // there is no bearer yet. Identity comes from the setup token in the body.

        [HttpPost]
        [Route("enroll/setup")]
        [AllowAnonymous]
        public Task<TotpSetupResponseDto> SetupAtLoginAsync(TotpSetupWithTokenDto input) =>
            _totpService.SetupAtLoginAsync(input);

        [HttpPost]
        [Route("enroll/confirm")]
        [AllowAnonymous]
        public Task<TotpEnrollResultDto> ConfirmAtLoginAsync(TotpConfirmWithTokenDto input) =>
            _totpService.ConfirmAtLoginAsync(input);

        [HttpPost]
        [Route("setup")]
        public async Task<TotpSetupResponseDto> SetupAsync()
        {
            return await _totpService.SetupAsync(GetCurrentUserId());
        }

        [HttpPost]
        [Route("confirm")]
        public async Task<TotpRecoveryCodesResponseDto> ConfirmAsync(TotpCodeRequestDto request)
        {
            return await _totpService.ConfirmAsync(GetCurrentUserId(), request.TotpCode);
        }

        [HttpPost]
        [Route("verify-login")]
        [AllowAnonymous]
        public async Task<Contract.Identity.UserManager.TokenDto> VerifyLoginAsync(TotpVerifyLoginRequestDto request)
        {
            return await _totpService.VerifyLoginAsync(request, GetClientIp());
        }

        [HttpPost]
        [Route("disable")]
        public async Task DisableAsync(TotpCodeRequestDto request)
        {
            await _totpService.DisableAsync(GetCurrentUserId(), request.TotpCode);
        }

        [HttpGet]
        [Route("status")]
        public async Task<TotpStatusDto> GetStatusAsync()
        {
            return await _totpService.GetStatusAsync(GetCurrentUserId());
        }

        [HttpPost]
        [Route("regenerate-recovery-codes")]
        public async Task<TotpRecoveryCodesResponseDto> RegenerateRecoveryCodesAsync(TotpCodeRequestDto request)
        {
            return await _totpService.RegenerateRecoveryCodesAsync(GetCurrentUserId(), request.TotpCode);
        }

        [HttpGet]
        [Route("admin/status/{userId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<TotpStatusDto> AdminGetStatusAsync(int userId)
        {
            return await _totpService.AdminGetStatusAsync(userId);
        }

        [HttpPost]
        [Route("admin/disable/{userId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task AdminDisableAsync(int userId)
        {
            await _totpService.AdminDisableAsync(userId);
        }

        [HttpPost]
        [Route("admin/regenerate-recovery-codes/{userId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<TotpRecoveryCodesResponseDto> AdminRegenerateRecoveryCodesAsync(int userId)
        {
            return await _totpService.AdminRegenerateRecoveryCodesAsync(userId);
        }

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

        private string GetClientIp()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
