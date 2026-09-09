using Blazored.LocalStorage;
using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IYubikeyApiService
{
    Task<YubikeyFeatureStatusDto?> GetFeatureStatusAsync();

    // Login, holding only the half-authenticated token
    Task<YubikeyPinResultDto?> VerifyPinAsync(string temporaryToken, string pin);
    Task<TokenDto?> VerifyOtpAsync(string temporaryToken, string otp);

    /// <summary>Registering the first key from the login screen, and the steps that follow it.</summary>
    Task<YubikeyEnrollResultDto?> EnrollAtLoginAsync(string enrollToken, string otp);
    Task<YubikeySetupProgressDto?> SetPinAtLoginAsync(string setupToken, string pin, string confirmPin);

    // Login with no password at all: the touch names the account, the PIN proves it
    Task<YubikeyPasswordlessTouchResultDto?> PasswordlessTouchAsync(string otp);
    Task<TokenDto?> PasswordlessPinAsync(string temporaryToken, string pin);

    // The owner, signed in
    Task<List<YubikeyDto>?> GetKeysAsync();
    Task EnrollAsync(string otp);
    Task SetPinAsync(string pin, string confirmPin);
    Task DeleteKeyAsync(int keyId);

    // Administration
    Task<AdminYubikeyStatusDto?> AdminGetStatusAsync(int userId);
    Task AdminResetAsync(int userId);
}

public class YubikeyApiService : IYubikeyApiService
{
    private readonly ILocalStorageService _localStorage;

    public YubikeyApiService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public Task<YubikeyFeatureStatusDto?> GetFeatureStatusAsync() =>
        RequestClient.GetAPIAsync<YubikeyFeatureStatusDto>("yubikey/feature-status");

    // ---------- login ----------
    //
    // No bearer is attached on this path on purpose. Identity comes from the temporary token in
    // the body: the browser sends whatever bearer is left in localStorage even on the login page,
    // and on a shared machine that is somebody else's session.

    public Task<YubikeyPinResultDto?> VerifyPinAsync(string temporaryToken, string pin) =>
        RequestClient.PostAPIAsync<YubikeyPinResultDto>("yubikey/pin/verify",
            new { temporaryToken, pin });

    public Task<TokenDto?> VerifyOtpAsync(string temporaryToken, string otp) =>
        RequestClient.PostAPIAsync<TokenDto>("yubikey/verify",
            new { temporaryToken, otp });

    public Task<YubikeyEnrollResultDto?> EnrollAtLoginAsync(string enrollToken, string otp) =>
        RequestClient.PostAPIAsync<YubikeyEnrollResultDto>("yubikey/enroll/login",
            new { enrollToken, otp });

    public Task<YubikeySetupProgressDto?> SetPinAtLoginAsync(string setupToken, string pin, string confirmPin) =>
        RequestClient.PostAPIAsync<YubikeySetupProgressDto>("yubikey/enroll/pin",
            new { setupToken, pin, confirmPin });

    public Task<YubikeyPasswordlessTouchResultDto?> PasswordlessTouchAsync(string otp) =>
        RequestClient.PostAPIAsync<YubikeyPasswordlessTouchResultDto>("yubikey/login/touch",
            new { otp });

    public Task<TokenDto?> PasswordlessPinAsync(string temporaryToken, string pin) =>
        RequestClient.PostAPIAsync<TokenDto>("yubikey/login/pin",
            new { temporaryToken, pin });

    // ---------- signed in ----------

    public async Task<List<YubikeyDto>?> GetKeysAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<List<YubikeyDto>>("yubikey/keys");
    }

    public async Task EnrollAsync(string otp)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.PostAPIAsync<YubikeyDto>("yubikey/enroll", new { otp });
    }

    public async Task SetPinAsync(string pin, string confirmPin)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.PostAPIAsync<object>("yubikey/pin", new { pin, confirmPin });
    }

    public async Task DeleteKeyAsync(int keyId)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.DeleteAPIAsync<object>($"yubikey/keys/{keyId}");
    }

    // ---------- administration ----------

    public async Task<AdminYubikeyStatusDto?> AdminGetStatusAsync(int userId)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<AdminYubikeyStatusDto>($"yubikey/admin/status/{userId}");
    }

    public async Task AdminResetAsync(int userId)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.PostAPIAsync<object>($"yubikey/admin/reset/{userId}", new { });
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
