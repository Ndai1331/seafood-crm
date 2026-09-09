using Contract.Identity.UserManager;

namespace Contract.Identity.Yubico
{
    public interface IYubikeyService
    {
        Task<YubikeyFeatureStatusDto> GetFeatureStatusAsync();
        Task<List<YubikeyDto>> GetKeysAsync(int userId);
        Task DeleteKeyAsync(int userId, int keyId);

        Task<AdminYubikeyStatusDto> AdminGetStatusAsync(int targetUserId);
        Task AdminResetAsync(int adminUserId, int targetUserId);

        /// <summary>One touch registers the key. No administrator step, nothing secret to hand over.</summary>
        Task<YubikeyDto> EnrollAsync(int userId, YubikeyEnrollDto input);
        Task SetPinAsync(int userId, YubikeySetPinDto input);

        /// <summary>
        /// The same registration, run from the login screen by someone who has cleared their
        /// password but owes a key. Returns the session too — that is the whole point of doing it
        /// here rather than sending them to the profile page they cannot reach yet.
        /// </summary>
        Task<YubikeyEnrollResultDto> EnrollAtLoginAsync(YubikeyEnrollWithTokenDto input);

        /// <summary>
        /// The PIN step of that same wizard. Holds a setup token, not a session — the session is
        /// what the wizard is working towards.
        /// </summary>
        Task<YubikeySetupProgressDto> SetPinAtLoginAsync(YubikeySetupPinDto input);

        Task<YubikeyPinResultDto> VerifyPinAsync(YubikeyPinDto input);
        Task<TokenDto> VerifyOtpAsync(YubikeyVerifyDto input);

        /// <summary>Signing in with no password: the touch names the account, the PIN proves it.</summary>
        Task<YubikeyPasswordlessTouchResultDto> PasswordlessTouchAsync(YubikeyPasswordlessTouchDto input);
        Task<TokenDto> PasswordlessPinAsync(YubikeyPasswordlessPinDto input);
    }
}
