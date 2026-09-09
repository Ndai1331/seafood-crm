using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Identity;
using BootstrapBlazor.Server.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BootstrapBlazor.Server.Components.Task9;

/// <summary>
/// The three switches that decide what the login screen offers.
///
/// Every one of them can lock people out, so none of them is saved silently: the page shows what
/// the API would refuse before the round trip, and after a save it re-reads the server's own state
/// rather than trusting what the switches were left showing.
/// </summary>
public partial class SsoSettings
{
    [Inject] private AuthenticationStateProvider? AuthStateProvider { get; set; }
    [Inject] private ToastService? ToastService { get; set; }
    [Inject] private ILogger<SsoSettings>? Logger { get; set; }
    [Inject] private IWebAuthnApiService? WebAuthnApi { get; set; }

    private bool IsAdmin { get; set; }

    private Data.WebAuthnSettingStatusDto? WebAuthnStatus { get; set; }
    private bool WebAuthnEnabled { get; set; }
    private bool WebAuthnEnforced { get; set; }
    private bool AllowPasswordLogin { get; set; } = true;

    /// <summary>Public half of the Yubico pair — round-trips normally.</summary>
    private string YubicoClientId { get; set; } = string.Empty;

    /// <summary>
    /// Write-only. Starts empty on every load and is cleared after a save: the server never sends
    /// the stored secret back, so anything sitting here would be a value this page invented.
    /// </summary>
    private string YubicoSecretKey { get; set; } = string.Empty;
    private bool IsSavingWebAuthn { get; set; }
    private string? WebAuthnLoadError { get; set; }

    /// <summary>The danger zone is super-admin only, matching what the endpoint enforces.</summary>
    private bool IsSuperAdmin { get; set; }
    /// <summary>
    /// What the wipe would actually remove: everyone holding a key except the super admins, whose
    /// keys it deliberately leaves behind. Derived from the two counts the server already sends
    /// rather than a third one — they come from the same pass, so the subtraction is exact.
    /// </summary>
    private int ResettableKeyCount =>
        Math.Max(0, (WebAuthnStatus?.UsersWithKey ?? 0) - (WebAuthnStatus?.SuperAdminsWithKey ?? 0));

    /// <summary>
    /// Who can actually get in, account by account. The page used to show "12/121" and nothing
    /// else, so an admin could see how many people held a key but never which ones — and had no
    /// way to hand the password door to the one person who still needed it.
    /// </summary>
    private List<Data.WebAuthnUserAccessDto> UserAccess { get; set; } = new();
    private bool IsLoadingUserAccess { get; set; }
    private string UserSearch { get; set; } = string.Empty;
    private string AccessFilter { get; set; } = "all";
    private int? TogglingUserId { get; set; }

    private int LockedOutCount => UserAccess.Count(u => u.KeyCount == 0 && !u.PasswordLoginAllowed);
    private int PasswordGrantedCount => UserAccess.Count(u => u.PasswordLoginAllowed);
    private int WithKeyCount => UserAccess.Count(u => u.KeyCount > 0);

    /// <summary>
    /// Filter and search run here rather than on the server: the whole list is 121 rows, and a
    /// round trip per keystroke would be slower than the filtering it replaces.
    /// </summary>
    private List<Data.WebAuthnUserAccessDto> VisibleUsers
    {
        get
        {
            IEnumerable<Data.WebAuthnUserAccessDto> rows = AccessFilter switch
            {
                "withkey" => UserAccess.Where(u => u.KeyCount > 0),
                "password" => UserAccess.Where(u => u.PasswordLoginAllowed),
                "locked" => UserAccess.Where(u => u.KeyCount == 0 && !u.PasswordLoginAllowed),
                _ => UserAccess
            };

            var term = UserSearch.Trim();
            if (term.Length > 0)
            {
                rows = rows.Where(u =>
                    u.UserName.Contains(term, StringComparison.OrdinalIgnoreCase)
                 || u.FullName.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            return rows.ToList();
        }
    }

    private bool HistoryOpen { get; set; }
    private bool IsLoadingHistory { get; set; }
    private bool IsRestoringYubico { get; set; }
    private List<Data.WebAuthnSettingHistoryDto> History { get; set; } = new();
    private bool ResetConfirmOpen { get; set; }
    private string ResetConfirmText { get; set; } = string.Empty;
    private bool IsResettingKeys { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsAdmin = await CheckAdminRole();
        IsSuperAdmin = await CheckSuperAdminRole();
        if (IsAdmin)
        {
            await LoadWebAuthnSettingsAsync();
            await LoadUserAccessAsync();
        }
    }

    private async Task LoadUserAccessAsync()
    {
        if (WebAuthnApi == null) return;

        IsLoadingUserAccess = true;
        try
        {
            UserAccess = await WebAuthnApi.GetUserAccessAsync() ?? new();
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Could not read the login-access list.");
            UserAccess = new();
        }
        finally
        {
            IsLoadingUserAccess = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Hands one account the password door, or takes it back. The row follows the server's answer
    /// rather than the switch, for the same reason the switches above do.
    /// </summary>
    private async Task TogglePasswordLoginAsync(Data.WebAuthnUserAccessDto row, bool allowed)
    {
        if (WebAuthnApi == null || TogglingUserId != null) return;

        TogglingUserId = row.UserId;
        StateHasChanged();

        try
        {
            var updated = await WebAuthnApi.SetPasswordLoginAllowedAsync(row.UserId, allowed);
            row.PasswordLoginAllowed = updated?.PasswordLoginAllowed ?? row.PasswordLoginAllowed;

            // The locked-out number on the settings box is computed server-side from this flag,
            // so re-read it rather than doing the arithmetic twice.
            await LoadWebAuthnSettingsAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Could not change the password permission for user {UserId}.", row.UserId);
            if (ToastService != null)
            {
                await ToastService.Error("Không đổi được", ApiErrorMessage.Describe(ex, "Không rõ lỗi."));
            }

            // Put the row back to what the server holds.
            await LoadUserAccessAsync();
        }
        finally
        {
            TogglingUserId = null;
            StateHasChanged();
        }
    }

    /// <summary>Loaded on demand: most visits to this page never open the history.</summary>
    private async Task ToggleHistoryAsync()
    {
        HistoryOpen = !HistoryOpen;
        if (!HistoryOpen || WebAuthnApi == null) return;

        IsLoadingHistory = true;
        StateHasChanged();

        try
        {
            History = await WebAuthnApi.GetSettingsHistoryAsync() ?? new();
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Could not read the login-settings history.");
            History = new();
        }
        finally
        {
            IsLoadingHistory = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Copies a previous Yubico pair back into the live settings — the reason the history is kept.
    /// Only the credentials move; the switches above stay where the admin left them.
    /// </summary>
    private async Task RestoreYubicoAsync(int historyId)
    {
        if (WebAuthnApi == null || IsRestoringYubico) return;

        IsRestoringYubico = true;
        StateHasChanged();

        try
        {
            await WebAuthnApi.RestoreYubicoAsync(historyId);

            // Both the settings and the history moved, so re-read both rather than patching here.
            await LoadWebAuthnSettingsAsync();
            History = await WebAuthnApi.GetSettingsHistoryAsync() ?? new();

            if (ToastService != null)
            {
                await ToastService.Success(
                    "Đã khôi phục cặp khoá Yubico",
                    "Cấu hình xác thực khoá quay về mốc đã chọn. Có hiệu lực sau tối đa 30 giây.");
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Could not restore the Yubico credentials.");
            if (ToastService != null)
            {
                await ToastService.Error("Không khôi phục được", ApiErrorMessage.Describe(ex, "Không rõ lỗi."));
            }
        }
        finally
        {
            IsRestoringYubico = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Wipes every registered key so the whole organisation re-registers through the current
    /// enrollment flow. The page only asks; the API decides whether it is safe — it refuses while
    /// password login is off, which is the one case where this would lock everybody out.
    /// </summary>
    private async Task ResetAllKeysAsync()
    {
        if (WebAuthnApi == null || IsResettingKeys) return;

        IsResettingKeys = true;
        StateHasChanged();

        try
        {
            var summary = await WebAuthnApi.ResetAllKeysAsync();
            ResetConfirmOpen = false;
            ResetConfirmText = string.Empty;

            // Re-read rather than patching the counts here: the same reason a save does it.
            await LoadWebAuthnSettingsAsync();

            if (ToastService != null)
            {
                await ToastService.Success(
                    "Đã xoá khoá",
                    $"{summary?.YubikeysRemoved ?? 0} YubiKey và {summary?.PasskeysRemoved ?? 0} passkey "
                  + $"của {summary?.UsersAffected ?? 0} tài khoản. Khoá của super admin giữ nguyên. "
                  + "Những người bị xoá sẽ đăng ký lại ở màn đăng nhập.");
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Could not reset every security key.");
            if (ToastService != null)
            {
                await ToastService.Error("Không xoá được", ApiErrorMessage.Describe(ex, "Không rõ lỗi."));
            }
        }
        finally
        {
            IsResettingKeys = false;
            StateHasChanged();
        }
    }

    private async Task<bool> CheckSuperAdminRole()
    {
        if (AuthStateProvider == null) return false;
        var state = await AuthStateProvider.GetAuthenticationStateAsync();

        // Every role claim, not the first one: an account carrying ADMIN and SUPER_ADMIN would
        // fail a FindFirst check depending on which order the token happened to list them in.
        return state.User.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Any(c => c.Value == "SUPER_ADMIN");
    }

    private async Task LoadWebAuthnSettingsAsync()
    {
        if (WebAuthnApi == null) return;

        try
        {
            WebAuthnStatus = await WebAuthnApi.GetSettingsAsync();
            ApplyFromServer();
            WebAuthnLoadError = WebAuthnStatus == null ? "Máy chủ không trả về dữ liệu." : null;
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Could not load the login switches");
            WebAuthnStatus = null;
            WebAuthnLoadError = ApiErrorMessage.Describe(ex, "Không rõ lỗi.");
        }
    }

    private async Task SaveWebAuthnSettingsAsync()
    {
        if (WebAuthnApi == null || IsSavingWebAuthn) return;

        IsSavingWebAuthn = true;
        try
        {
            // The API re-checks both lockout guards and refuses on its own; the message it sends
            // back is the one worth showing, so it is surfaced verbatim rather than replaced.
            WebAuthnStatus = await WebAuthnApi.UpdateSettingsAsync(
                WebAuthnEnabled, WebAuthnEnforced, AllowPasswordLogin,
                YubicoClientId, YubicoSecretKey);
            ApplyFromServer();

            if (ToastService != null)
            {
                await ToastService.Success("Đã lưu", DescribeState());
            }
        }
        catch (Exception ex)
        {
            if (ToastService != null)
            {
                await ToastService.Error("Không lưu được", ApiErrorMessage.Describe(ex, "Không rõ lỗi."));
            }

            // Put the switches back to what the server actually holds, so the page never shows a
            // state that was refused.
            await LoadWebAuthnSettingsAsync();
        }
        finally
        {
            IsSavingWebAuthn = false;
            StateHasChanged();
        }
    }

    /// <summary>Switches follow the server, never the other way round.</summary>
    private void ApplyFromServer()
    {
        WebAuthnEnabled = WebAuthnStatus?.IsEnabled ?? false;
        WebAuthnEnforced = WebAuthnStatus?.IsEnforced ?? false;
        AllowPasswordLogin = WebAuthnStatus?.AllowPasswordLogin ?? true;
        YubicoClientId = WebAuthnStatus?.YubicoClientId ?? string.Empty;
        // Never prefilled: the API answers with HasYubicoSecret, not the secret.
        YubicoSecretKey = string.Empty;
    }

    private string DescribeState()
    {
        if (!WebAuthnEnabled)
        {
            return "Khoá bảo mật đang tắt. Đăng nhập bằng mật khẩu như trước.";
        }

        var keys = WebAuthnEnforced
            ? "Khoá bảo mật đang BẮT BUỘC với mọi user."
            : "Khoá bảo mật đang bật, user tự đăng ký.";

        return AllowPasswordLogin
            ? keys
            : $"{keys} Đăng nhập bằng mật khẩu đã TẮT — chỉ còn khoá bảo mật vào được.";
    }

    private async Task<bool> CheckAdminRole()
    {
        if (AuthStateProvider == null) return false;
        var state = await AuthStateProvider.GetAuthenticationStateAsync();
        var role = state.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        return role is "ADMIN" or "HEAD";
    }
}
