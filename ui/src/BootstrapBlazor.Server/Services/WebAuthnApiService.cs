using Blazored.LocalStorage;
using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootstrapBlazor.Server.Services;

public interface IWebAuthnApiService
{
    Task<WebAuthnFeatureStatusDto?> GetFeatureStatusAsync();

    // Signed-in registration (profile page)
    Task<WebAuthnCeremonyOptionsDto?> BeginRegistrationAsync();
    Task<SecurityKeyEnrollResultDto?> CompleteRegistrationAsync(string attestationJson, string deviceName);

    // First-key enrollment (no session yet)
    Task<WebAuthnCeremonyOptionsDto?> BeginEnrollmentAsync(string enrollToken);
    Task<TokenDto?> CompleteEnrollmentAsync(string enrollToken, string attestationJson, string deviceName);

    // Login
    Task<WebAuthnCeremonyOptionsDto?> BeginLoginAsync(string temporaryToken);
    Task<TokenDto?> CompleteLoginAsync(string temporaryToken, string assertionJson);

    // Key management
    Task<List<SecurityKeyDto>?> GetKeysAsync();
    Task DeleteKeyAsync(int keyId);

    // Admin
    Task<AdminSecurityKeyStatusDto?> AdminGetStatusAsync(int userId);
    Task AdminResetAsync(int userId);

    /// <summary>Stores a session the browser-driven ceremony already obtained.</summary>
    Task<TokenDto?> PersistSessionAsync(TokenDto token, string provider);

    // Switches
    Task<WebAuthnSettingStatusDto?> GetSettingsAsync();

    /// <summary>Removes every registered key in the organisation. Super admins only, server-side.</summary>
    Task<SecurityKeyResetSummaryDto?> ResetAllKeysAsync();

    /// <summary>
    /// Whether this account may be shown a password box while the organisation-wide switch is off.
    /// Anonymous — it runs on the login screen, before anyone has a token.
    /// </summary>
    Task<PasswordDoorDto?> CheckPasswordDoorAsync(string userName);

    /// <summary>Every active account with its key count and its password permission.</summary>
    Task<List<WebAuthnUserAccessDto>?> GetUserAccessAsync();

    /// <summary>Hands one account the password door, or takes it back.</summary>
    Task<WebAuthnUserAccessDto?> SetPasswordLoginAllowedAsync(int userId, bool allowed);

    /// <summary>The saved states of the login settings, newest first.</summary>
    Task<List<WebAuthnSettingHistoryDto>?> GetSettingsHistoryAsync();

    /// <summary>Puts the Yubico pair from one of those states back.</summary>
    Task<WebAuthnSettingStatusDto?> RestoreYubicoAsync(int historyId);
    Task<WebAuthnSettingStatusDto?> UpdateSettingsAsync(
        bool isEnabled, bool isEnforced, bool allowPasswordLogin,
        string? yubicoClientId, string? yubicoSecretKey);
}

public class WebAuthnApiService : IWebAuthnApiService
{
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public WebAuthnApiService(ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
    {
        _localStorage = localStorage;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public Task<WebAuthnFeatureStatusDto?> GetFeatureStatusAsync() =>
        RequestClient.GetAPIAsync<WebAuthnFeatureStatusDto>("webauthn/feature-status");

    // ---------- signed-in registration ----------

    public async Task<WebAuthnCeremonyOptionsDto?> BeginRegistrationAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<WebAuthnCeremonyOptionsDto>("webauthn/register/begin", new { });
    }

    public async Task<SecurityKeyEnrollResultDto?> CompleteRegistrationAsync(string attestationJson, string deviceName)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<SecurityKeyEnrollResultDto>("webauthn/register/complete",
            new WebAuthnRegisterCompleteDto { AttestationJson = attestationJson, DeviceName = deviceName });
    }

    // ---------- enrollment ----------
    // No EnsureRequestAuthAsync on this path: identity comes from the enroll token in the body and
    // the API ignores the header. Attaching a stale bearer here is exactly the shared-machine
    // hazard the split routes exist to prevent.

    public Task<WebAuthnCeremonyOptionsDto?> BeginEnrollmentAsync(string enrollToken) =>
        RequestClient.PostAPIAsync<WebAuthnCeremonyOptionsDto>("webauthn/enroll/begin",
            new WebAuthnCeremonyStartDto { TemporaryToken = enrollToken });

    public async Task<TokenDto?> CompleteEnrollmentAsync(string enrollToken, string attestationJson, string deviceName)
    {
        var result = await RequestClient.PostAPIAsync<SecurityKeyEnrollResultDto>("webauthn/enroll/complete",
            new WebAuthnEnrollCompleteDto
            {
                EnrollToken = enrollToken,
                AttestationJson = attestationJson,
                DeviceName = deviceName
            });

        return result?.Token == null ? null : await PersistSessionAsync(result.Token, "webauthn-enroll");
    }

    // ---------- login ----------

    public Task<WebAuthnCeremonyOptionsDto?> BeginLoginAsync(string temporaryToken) =>
        RequestClient.PostAPIAsync<WebAuthnCeremonyOptionsDto>("webauthn/login/begin",
            new WebAuthnCeremonyStartDto { TemporaryToken = temporaryToken });

    public async Task<TokenDto?> CompleteLoginAsync(string temporaryToken, string assertionJson)
    {
        var response = await RequestClient.PostAPIAsync<TokenDto>("webauthn/login/complete",
            new WebAuthnLoginCompleteDto { TemporaryToken = temporaryToken, AssertionJson = assertionJson });

        return response == null ? null : await PersistSessionAsync(response, "webauthn");
    }

    // ---------- key management ----------

    public async Task<List<SecurityKeyDto>?> GetKeysAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<List<SecurityKeyDto>>("webauthn/keys");
    }

    public async Task DeleteKeyAsync(int keyId)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.DeleteAPIAsync<object>($"webauthn/keys/{keyId}");
    }

    // ---------- admin ----------

    public async Task<AdminSecurityKeyStatusDto?> AdminGetStatusAsync(int userId)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<AdminSecurityKeyStatusDto>($"webauthn/admin/status/{userId}");
    }

    public async Task AdminResetAsync(int userId)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.PostAPIAsync<object>($"webauthn/admin/reset/{userId}", new { });
    }

    // ---------- switches ----------

    public async Task<List<WebAuthnSettingHistoryDto>?> GetSettingsHistoryAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<List<WebAuthnSettingHistoryDto>>("webauthn/settings/history");
    }

    public async Task<WebAuthnSettingStatusDto?> RestoreYubicoAsync(int historyId)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<WebAuthnSettingStatusDto>(
            $"webauthn/settings/history/{historyId}/restore-yubico", new { });
    }

    public async Task<SecurityKeyResetSummaryDto?> ResetAllKeysAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<SecurityKeyResetSummaryDto>(
            "webauthn/admin/reset-all-keys", new { });
    }

    public Task<PasswordDoorDto?> CheckPasswordDoorAsync(string userName) =>
        RequestClient.GetAPIAsync<PasswordDoorDto>(
            $"webauthn/password-door?userName={Uri.EscapeDataString(userName)}");

    public async Task<List<WebAuthnUserAccessDto>?> GetUserAccessAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<List<WebAuthnUserAccessDto>>("webauthn/settings/users");
    }

    public async Task<WebAuthnUserAccessDto?> SetPasswordLoginAllowedAsync(int userId, bool allowed)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PutAPIAsync<WebAuthnUserAccessDto>(
            $"webauthn/settings/users/{userId}/password-login", new { Allowed = allowed });
    }

    public async Task<WebAuthnSettingStatusDto?> GetSettingsAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<WebAuthnSettingStatusDto>("webauthn/settings");
    }

    public async Task<WebAuthnSettingStatusDto?> UpdateSettingsAsync(
        bool isEnabled, bool isEnforced, bool allowPasswordLogin,
        string? yubicoClientId, string? yubicoSecretKey)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PutAPIAsync<WebAuthnSettingStatusDto>("webauthn/settings",
            new WebAuthnSettingUpdateDto
            {
                IsEnabled = isEnabled,
                IsEnforced = isEnforced,
                AllowPasswordLogin = allowPasswordLogin,
                YubicoClientId = yubicoClientId,
                YubicoSecretKey = yubicoSecretKey
            });
    }

    public async Task<TokenDto?> PersistSessionAsync(TokenDto response, string provider)
    {
        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            return null;
        }

        RequestClient.InjectServices(_localStorage);
        await RequestClient.RunTokenMutationAsync(async () =>
        {
            await _localStorage.SetItemAsync("my-access-token", response.AccessToken);
            await _localStorage.SetItemAsync("my-refresh-token", response.RefreshToken);
            RequestClient.AttachToken(response.AccessToken);
        });

        await ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(provider);
        return response;
    }

    private async Task EnsureRequestAuthAsync()
    {
        RequestClient.InjectServices(_localStorage);
        var token = await _localStorage.GetItemAsync<string>("my-access-token");
        if (!string.IsNullOrWhiteSpace(token))
        {
            RequestClient.AttachToken(token);
        }
    }
}
