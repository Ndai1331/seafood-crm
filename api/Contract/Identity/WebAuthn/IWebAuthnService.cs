using Contract.Identity.UserManager;

namespace Contract.Identity.WebAuthn
{
    public interface IWebAuthnService
    {
        Task<WebAuthnFeatureStatusDto> GetFeatureStatusAsync();

        // --- registration by a signed-in user (profile page) ---
        Task<WebAuthnCeremonyOptionsDto> BeginRegistrationAsync(int userId);
        Task<SecurityKeyEnrollResultDto> CompleteRegistrationAsync(int userId, WebAuthnRegisterCompleteDto input);

        // --- first-key enrollment, before a session exists ---
        Task<WebAuthnCeremonyOptionsDto> BeginEnrollmentAsync(string enrollToken);
        Task<SecurityKeyEnrollResultDto> CompleteEnrollmentAsync(WebAuthnEnrollCompleteDto input);

        // --- login ---
        Task<WebAuthnCeremonyOptionsDto> BeginLoginAsync(string temporaryToken);
        Task<TokenDto> CompleteLoginAsync(WebAuthnLoginCompleteDto input);

        // --- key management ---
        Task<List<SecurityKeyDto>> GetKeysAsync(int userId);
        Task DeleteKeyAsync(int userId, int keyId);

        // --- admin ---
        Task<AdminSecurityKeyStatusDto> AdminGetStatusAsync(int targetUserId);
        Task AdminResetAsync(int adminUserId, int targetUserId);
    }
}
