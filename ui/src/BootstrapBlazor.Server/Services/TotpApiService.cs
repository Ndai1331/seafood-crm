using Blazored.LocalStorage;
using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Http;
using BootstrapBlazor.Server.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootstrapBlazor.Server.Services;

public interface ITotpApiService
{
    Task<bool> IsFeatureEnabledAsync();
    Task<TotpStatusDto?> GetStatusAsync();
    Task<TotpSetupResponseDto?> SetupAsync();

    /// <summary>The authenticator steps run from the login screen, on a setup token.</summary>
    Task<TotpSetupResponseDto?> SetupAtLoginAsync(string setupToken);
    Task<TotpEnrollResultDto?> ConfirmAtLoginAsync(string setupToken, string totpCode);

    Task<TotpRecoveryCodesResponseDto?> ConfirmAsync(string totpCode);
    Task<TokenDto?> VerifyLoginAsync(string temporaryToken, string? totpCode, string? recoveryCode);
    Task DisableAsync(string totpCode);
    Task<TotpRecoveryCodesResponseDto?> RegenerateRecoveryCodesAsync(string totpCode);
    Task<TotpStatusDto?> AdminGetStatusAsync(int userId);
    Task AdminDisableAsync(int userId);
    Task<TotpRecoveryCodesResponseDto?> AdminRegenerateRecoveryCodesAsync(int userId);
}

public class TotpApiService : ITotpApiService
{
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public TotpApiService(ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
    {
        _localStorage = localStorage;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> IsFeatureEnabledAsync()
    {
        var status = await RequestClient.GetAPIAsync<TotpFeatureStatusDto>("totp/feature-status");
        return status?.FeatureEnabled == true;
    }

    // No bearer on these two: the session is what they are working towards, and identity comes
    // from the setup token in the body.
    public Task<TotpSetupResponseDto?> SetupAtLoginAsync(string setupToken) =>
        RequestClient.PostAPIAsync<TotpSetupResponseDto>("totp/enroll/setup", new { setupToken });

    public Task<TotpEnrollResultDto?> ConfirmAtLoginAsync(string setupToken, string totpCode) =>
        RequestClient.PostAPIAsync<TotpEnrollResultDto>("totp/enroll/confirm",
            new { setupToken, totpCode });

    public async Task<TotpStatusDto?> GetStatusAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<TotpStatusDto>("totp/status");
    }

    public async Task<TotpSetupResponseDto?> SetupAsync()
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<TotpSetupResponseDto>("totp/setup", new { });
    }

    public async Task<TotpRecoveryCodesResponseDto?> ConfirmAsync(string totpCode)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<TotpRecoveryCodesResponseDto>("totp/confirm", new TotpCodeRequestDto { TotpCode = totpCode });
    }

    public async Task<TokenDto?> VerifyLoginAsync(string temporaryToken, string? totpCode, string? recoveryCode)
    {
        System.Console.WriteLine($"[AuthFlow] TOTP verify-login start temporary:{RequestClient.DescribeToken(temporaryToken)} useRecovery:{!string.IsNullOrWhiteSpace(recoveryCode)}");
        var response = await RequestClient.PostAPIAsync<TokenDto>("totp/verify-login", new TotpVerifyLoginRequestDto
        {
            TemporaryToken = temporaryToken,
            TotpCode = string.IsNullOrWhiteSpace(recoveryCode) ? totpCode : null,
            RecoveryCode = string.IsNullOrWhiteSpace(recoveryCode) ? null : recoveryCode
        });
        System.Console.WriteLine($"[AuthFlow] TOTP verify-login response null:{response == null} access:{RequestClient.DescribeToken(response?.AccessToken)} refreshMissing:{string.IsNullOrWhiteSpace(response?.RefreshToken)}");

        if (response == null || string.IsNullOrWhiteSpace(response.AccessToken))
        {
            System.Console.WriteLine("[AuthFlow] TOTP verify-login stopped: access token missing");
            return null;
        }

        RequestClient.InjectServices(_localStorage);
        await RequestClient.RunTokenMutationAsync(async () =>
        {
            await _localStorage.SetItemAsync("my-access-token", response.AccessToken);
            await _localStorage.SetItemAsync("my-refresh-token", response.RefreshToken);
            RequestClient.AttachToken(response.AccessToken);
        });
        var savedAccessToken = await _localStorage.GetItemAsync<string>("my-access-token");
        var savedRefreshToken = await _localStorage.GetItemAsync<string>("my-refresh-token");
        System.Console.WriteLine($"[AuthFlow] TOTP verify-login saved localStorage access:{RequestClient.DescribeToken(savedAccessToken)} refreshMissing:{string.IsNullOrWhiteSpace(savedRefreshToken)}");
        await ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated("totp");
        System.Console.WriteLine("[AuthFlow] TOTP verify-login MarkUserAsAuthenticated completed");
        return response;
    }

    public async Task DisableAsync(string totpCode)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.PostAPIAsync<object>("totp/disable", new TotpCodeRequestDto { TotpCode = totpCode });
    }

    public async Task<TotpRecoveryCodesResponseDto?> RegenerateRecoveryCodesAsync(string totpCode)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<TotpRecoveryCodesResponseDto>("totp/regenerate-recovery-codes", new TotpCodeRequestDto { TotpCode = totpCode });
    }

    public async Task<TotpStatusDto?> AdminGetStatusAsync(int userId)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.GetAPIAsync<TotpStatusDto>($"totp/admin/status/{userId}");
    }

    public async Task AdminDisableAsync(int userId)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.PostAPIAsync<object>($"totp/admin/disable/{userId}", new { });
    }

    public async Task<TotpRecoveryCodesResponseDto?> AdminRegenerateRecoveryCodesAsync(int userId)
    {
        await EnsureRequestAuthAsync();
        return await RequestClient.PostAPIAsync<TotpRecoveryCodesResponseDto>($"totp/admin/regenerate-recovery-codes/{userId}", new { });
    }

    private async Task EnsureRequestAuthAsync()
    {
        RequestClient.InjectServices(_localStorage);
        var token = await _localStorage.GetItemAsync<string>("my-access-token");
        System.Console.WriteLine($"[AuthFlow] TotpApiService.EnsureRequestAuth {RequestClient.DescribeToken(token)}");
        if (!string.IsNullOrWhiteSpace(token))
        {
            RequestClient.AttachToken(token);
        }
    }
}
