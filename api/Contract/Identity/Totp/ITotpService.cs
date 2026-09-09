using Contract.Identity.UserManager;

namespace Contract.Identity.Totp
{
    public interface ITotpService
    {
        Task<TotpSetupResponseDto> SetupAsync(int userId);
        Task<TotpRecoveryCodesResponseDto> ConfirmAsync(int userId, string totpCode);

        /// <summary>The same two steps run from the login screen, holding a setup token.</summary>
        Task<TotpSetupResponseDto> SetupAtLoginAsync(TotpSetupWithTokenDto input);
        Task<TotpEnrollResultDto> ConfirmAtLoginAsync(TotpConfirmWithTokenDto input);
        Task<TokenDto> VerifyLoginAsync(TotpVerifyLoginRequestDto request, string clientIp);
        Task DisableAsync(int userId, string totpCode);
        Task<TotpStatusDto> GetStatusAsync(int userId);
        Task<TotpRecoveryCodesResponseDto> RegenerateRecoveryCodesAsync(int userId, string totpCode);
        Task<TotpStatusDto> AdminGetStatusAsync(int userId);
        Task AdminDisableAsync(int userId);
        Task<TotpRecoveryCodesResponseDto> AdminRegenerateRecoveryCodesAsync(int userId);
    }
}
