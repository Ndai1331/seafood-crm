using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using BootstrapBlazor.Server.Services;
using BootstrapBlazor.Server.Data;
using BootstrapBlazor.Server.Identity;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Components.Task9.MyProfile;

/// <summary>
/// Code-behind cho trang thông tin cá nhân.
/// Cho phép mọi user đã đăng nhập xem và cập nhật thông tin cá nhân.
/// </summary>
public partial class MyProfile : ComponentBase
{
    [Inject]
    [NotNull]
    private IUserManagerService? UserManagerService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }

    [Inject, NotNull]
    private IServiceProvider? ServiceProvider { get; set; }

    [Inject]
    private IJSRuntime? JSRuntime { get; set; }

    [Inject]
    private ITotpApiService? TotpApiService { get; set; }

    [Inject]
    private IWebAuthnApiService? WebAuthnApiService { get; set; }

    // Form model (chỉ chứa các trường user được sửa)
    private UpdateProfileDto? Model { get; set; }

    // Loading states
    private bool IsLoading { get; set; } = false;
    private bool IsLoadingData { get; set; } = true;
    private bool ProfileInputsStyled { get; set; }

    /// <summary>Reported by YubikeySection; -1 until its first load lands so the one-way-in
    /// warning cannot fire off incomplete numbers.</summary>
    private int YubikeyKeyCount { get; set; } = -1;

    /// <summary>
    /// Every second factor this account can present, counted across all three cards. The number
    /// the warning below cares about: at one, losing that one means an administrator visit.
    /// </summary>
    private int SecondFactorCount =>
        Math.Max(YubikeyKeyCount, 0) + SecurityKeys.Count + (TotpEnabled ? 1 : 0);

    private bool SecondFactorCountReady => YubikeyKeyCount >= 0 && TotpStatusLoaded;

    private Task OnYubikeyKeysCounted(int count)
    {
        YubikeyKeyCount = count;
        StateHasChanged();
        return Task.CompletedTask;
    }

    // Current user info
    private int CurrentUserId { get; set; }

    // Read-only display fields
    private string FullName { get; set; } = "";
    private string UserCode { get; set; } = "";
    private string DisplayRole { get; set; } = "";
    private string DisplayPosition { get; set; } = "";
    private string DisplayTeam { get; set; } = "";
    private string AvatarUrl { get; set; } = "";
    private string Initials { get; set; } = "?";

    private bool TotpFeatureEnabled { get; set; }
    private bool TotpStatusLoaded { get; set; }
    private bool TotpEnabled { get; set; }
    private bool IsLoadingTotp { get; set; }
    private bool IsTotpBusy { get; set; }
    private int RemainingRecoveryCodes { get; set; }
    private TotpSetupResponseDto? TotpSetup { get; set; }
    private string TotpConfirmCode { get; set; } = "";
    private string TotpActionCode { get; set; } = "";

    private bool SecurityKeyFeatureEnabled { get; set; }
    private bool SecurityKeyEnforced { get; set; }
    private bool IsLoadingSecurityKeys { get; set; }
    private bool IsSecurityKeyBusy { get; set; }
    private List<SecurityKeyDto> SecurityKeys { get; set; } = new();
    private IJSObjectReference? WebAuthnModule { get; set; }
    private DotNetObjectReference<MyProfile>? WebAuthnRef { get; set; }
    private bool WebAuthnBound { get; set; }

    /// <summary>Stable id so the JS side can attach a native click listener to this exact button.</summary>
    private const string RegisterKeyButtonId = "webauthn-register-key";

    /// <summary>Removing this key would lock the account out; the API refuses it too.</summary>
    private bool IsLastKeyWhileEnforced => SecurityKeyEnforced && SecurityKeys.Count == 1;

    private List<string> RecoveryCodes { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadProfile();
    }

    /// <summary>
    /// Force uniform input sizing via JS interop (same pattern as EditUser)
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        // The backup-key card put the WebAuthn ceremony back on this page (2026-08-24) — as the
        // spare key, not the main door. The button only exists once that card has rendered, so
        // binding retries each render until it takes; the JS side is idempotent and WebAuthnBound
        // stops the repeat calls.
        await BindWebAuthnButtonAsync();

        // Runs once, and only after the form is actually on screen — it cannot key off
        // firstRender because the page still shows the spinner at that point. Left unguarded
        // this cost 50ms plus a DOM walk over SignalR on every single render.
        if (JSRuntime != null && !ProfileInputsStyled && !IsLoadingData && Model != null)
        {
            ProfileInputsStyled = true;
            await Task.Delay(50);
            try
            {
                var jsCode = @"
                    (function() {
                        const PAGE_ID = '#page-my-profile';
                        
                        // === 1. BootstrapInput — input.form-control ===
                        const inputs = document.querySelectorAll(PAGE_ID + ' input.form-control.seo-input-size, ' + PAGE_ID + ' .seo-input-size input.form-control');
                        inputs.forEach(input => {
                            input.style.setProperty('height', '54px', 'important');
                            input.style.setProperty('min-height', '54px', 'important');
                            input.style.setProperty('max-height', '54px', 'important');
                            input.style.setProperty('padding', '18px 14px 16px 14px', 'important');
                            input.style.setProperty('box-sizing', 'border-box', 'important');
                            input.style.setProperty('font-size', '15px', 'important');
                            input.style.setProperty('line-height', '22px', 'important');
                            input.style.setProperty('margin-top', '0', 'important');
                            input.style.setProperty('border-radius', '10px', 'important');
                        });
                        
                        // === 2. Dropdown-toggle (Select, MultiSelect) — OUTER gets border ===
                        const dropdownToggles = document.querySelectorAll(PAGE_ID + ' .dropdown-toggle.seo-input-size, ' + PAGE_ID + ' .seo-input-size.dropdown-toggle, ' + PAGE_ID + ' .seo-input-size .dropdown-toggle');
                        dropdownToggles.forEach(toggle => {
                            toggle.style.setProperty('height', '54px', 'important');
                            toggle.style.setProperty('min-height', '54px', 'important');
                            toggle.style.setProperty('max-height', '54px', 'important');
                            toggle.style.setProperty('padding', '18px 14px 16px 14px', 'important');
                            toggle.style.setProperty('box-sizing', 'border-box', 'important');
                            toggle.style.setProperty('font-size', '15px', 'important');
                            toggle.style.setProperty('line-height', '22px', 'important');
                            toggle.style.setProperty('margin-top', '0', 'important');
                            toggle.style.setProperty('display', 'flex', 'important');
                            toggle.style.setProperty('align-items', 'center', 'important');
                            toggle.style.setProperty('overflow', 'hidden', 'important');
                            toggle.style.setProperty('border', '0.5px solid rgba(145, 158, 171, 0.2)', 'important');
                            toggle.style.setProperty('border-radius', '10px', 'important');
                            
                            // Remove border of INNER inputs/form-select inside dropdown-toggle → prevent double border
                            const innerInputs = toggle.querySelectorAll('input, .form-control, .form-select, input.form-control, input.form-select, .form-select.form-control');
                            innerInputs.forEach(innerInput => {
                                innerInput.style.removeProperty('border');
                                innerInput.style.removeProperty('border-width');
                                innerInput.style.removeProperty('border-style');
                                innerInput.style.removeProperty('border-color');
                                
                                innerInput.style.setProperty('border', 'none', 'important');
                                innerInput.style.setProperty('border-width', '0', 'important');
                                innerInput.style.setProperty('border-style', 'none', 'important');
                                innerInput.style.setProperty('border-color', 'transparent', 'important');
                                innerInput.style.setProperty('background', 'transparent', 'important');
                                innerInput.style.setProperty('padding', '0', 'important');
                                innerInput.style.setProperty('margin', '0', 'important');
                                innerInput.style.setProperty('height', '100%', 'important');
                                innerInput.style.setProperty('box-shadow', 'none', 'important');
                                innerInput.style.setProperty('outline', 'none', 'important');
                            });
                        });
                        
                        // === 3. DateTimePicker ===
                        const datetimePickerInputs = document.querySelectorAll(PAGE_ID + ' .datetime-picker-input, ' + PAGE_ID + ' input.datetime-picker-input, ' + PAGE_ID + ' .dropdown-toggle.datetime-picker-input, ' + PAGE_ID + ' .datetime-picker .datetime-picker-input');
                        datetimePickerInputs.forEach(input => {
                            input.style.removeProperty('padding');
                            input.style.removeProperty('padding-top');
                            input.style.removeProperty('padding-right');
                            input.style.removeProperty('padding-bottom');
                            input.style.removeProperty('padding-left');
                            
                            input.style.setProperty('height', '54px', 'important');
                            input.style.setProperty('min-height', '54px', 'important');
                            input.style.setProperty('max-height', '54px', 'important');
                            input.style.setProperty('padding-top', '18px', 'important');
                            input.style.setProperty('padding-right', '14px', 'important');
                            input.style.setProperty('padding-bottom', '16px', 'important');
                            input.style.setProperty('padding-left', '40px', 'important');
                            input.style.setProperty('box-sizing', 'border-box', 'important');
                            input.style.setProperty('font-size', '15px', 'important');
                            input.style.setProperty('line-height', '22px', 'important');
                            input.style.setProperty('margin-top', '0', 'important');
                            input.style.setProperty('border', '0.5px solid rgba(145, 158, 171, 0.2)', 'important');
                            input.style.setProperty('border-radius', '10px', 'important');
                            input.style.setProperty('display', 'flex', 'important');
                            input.style.setProperty('align-items', 'center', 'important');
                        });
                        
                        // DateTimePicker bar icon
                        const datetimePickerIcons = document.querySelectorAll(PAGE_ID + ' .datetime-picker-bar, ' + PAGE_ID + ' .datetime-picker .datetime-picker-bar');
                        datetimePickerIcons.forEach(icon => {
                            icon.style.setProperty('position', 'absolute', 'important');
                            icon.style.setProperty('left', '14px', 'important');
                            icon.style.setProperty('top', '50%', 'important');
                            icon.style.setProperty('transform', 'translateY(-50%)', 'important');
                            icon.style.setProperty('line-height', '1', 'important');
                            icon.style.setProperty('display', 'flex', 'important');
                            icon.style.setProperty('align-items', 'center', 'important');
                            icon.style.setProperty('justify-content', 'center', 'important');
                            icon.style.setProperty('z-index', '10', 'important');
                            icon.style.setProperty('pointer-events', 'none', 'important');
                        });

                        // === 4. Buttons ===
                        const buttons = document.querySelectorAll(PAGE_ID + ' .btn.seo-input-size, ' + PAGE_ID + ' button.seo-input-size');
                        buttons.forEach(btn => {
                            btn.style.setProperty('height', '54px', 'important');
                            btn.style.setProperty('min-height', '54px', 'important');
                            btn.style.setProperty('max-height', '54px', 'important');
                            btn.style.setProperty('padding', '18px 14px 16px 14px', 'important');
                            btn.style.setProperty('box-sizing', 'border-box', 'important');
                            btn.style.setProperty('font-size', '15px', 'important');
                            btn.style.setProperty('line-height', '22px', 'important');
                            btn.style.setProperty('margin-top', '0', 'important');
                            btn.style.setProperty('display', 'flex', 'important');
                            btn.style.setProperty('align-items', 'center', 'important');
                            btn.style.setProperty('justify-content', 'center', 'important');
                        });

                        // === Retry at 100ms — re-clear inner borders (Bootstrap may re-render) ===
                        setTimeout(() => {
                            const dropdownToggles2 = document.querySelectorAll(PAGE_ID + ' .dropdown-toggle.seo-input-size, ' + PAGE_ID + ' .seo-input-size.dropdown-toggle, ' + PAGE_ID + ' .seo-input-size .dropdown-toggle');
                            dropdownToggles2.forEach(toggle => {
                                const innerInputs2 = toggle.querySelectorAll('input, .form-control, .form-select, input.form-control, input.form-select, .form-select.form-control');
                                innerInputs2.forEach(innerInput => {
                                    innerInput.style.removeProperty('border');
                                    innerInput.style.removeProperty('border-width');
                                    innerInput.style.removeProperty('border-style');
                                    innerInput.style.removeProperty('border-color');
                                    
                                    innerInput.style.setProperty('border', 'none', 'important');
                                    innerInput.style.setProperty('border-width', '0', 'important');
                                    innerInput.style.setProperty('border-style', 'none', 'important');
                                    innerInput.style.setProperty('border-color', 'transparent', 'important');
                                    innerInput.style.setProperty('background', 'transparent', 'important');
                                    innerInput.style.setProperty('padding', '0', 'important');
                                    innerInput.style.setProperty('margin', '0', 'important');
                                    innerInput.style.setProperty('height', '100%', 'important');
                                    innerInput.style.setProperty('box-shadow', 'none', 'important');
                                    innerInput.style.setProperty('outline', 'none', 'important');
                                });
                            });
                            
                            // Re-apply datetime padding
                            const dtInputs = document.querySelectorAll(PAGE_ID + ' .datetime-picker-input');
                            dtInputs.forEach(input => {
                                input.style.removeProperty('padding');
                                input.style.setProperty('padding-top', '18px', 'important');
                                input.style.setProperty('padding-right', '14px', 'important');
                                input.style.setProperty('padding-bottom', '16px', 'important');
                                input.style.setProperty('padding-left', '40px', 'important');
                                input.style.setProperty('height', '54px', 'important');
                            });
                        }, 100);
                        
                        // === Retry at 300ms ===
                        setTimeout(() => {
                            const dtInputs = document.querySelectorAll(PAGE_ID + ' .datetime-picker-input');
                            dtInputs.forEach(input => {
                                input.style.removeProperty('padding');
                                input.style.setProperty('padding-top', '18px', 'important');
                                input.style.setProperty('padding-right', '14px', 'important');
                                input.style.setProperty('padding-bottom', '16px', 'important');
                                input.style.setProperty('padding-left', '40px', 'important');
                            });
                        }, 300);
                        
                        // === Retry at 500ms ===
                        setTimeout(() => {
                            const dtInputs = document.querySelectorAll(PAGE_ID + ' .datetime-picker-input');
                            dtInputs.forEach(input => {
                                input.style.removeProperty('padding');
                                input.style.setProperty('padding-top', '18px', 'important');
                                input.style.setProperty('padding-right', '14px', 'important');
                                input.style.setProperty('padding-bottom', '16px', 'important');
                                input.style.setProperty('padding-left', '40px', 'important');
                            });
                        }, 500);
                    })();
                ";
                await JSRuntime.InvokeVoidAsync("eval", jsCode);
            }
            catch
            {
                // Ignore JS errors
            }
        }
    }

    /// <summary>
    /// Lấy current user ID từ JWT token và load thông tin profile
    /// </summary>
    private async Task LoadProfile()
    {
        IsLoadingData = true;
        try
        {
            // Get current user ID from JWT
            var provider = ServiceProvider.GetService<AuthenticationStateProvider>();
            var userIdStr = await ((ApiAuthenticationStateProvider)provider!).GetCurrentUserId();

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                await ToastService.Error("Lỗi", "Không xác định được người dùng hiện tại");
                NavigationManager.NavigateTo("/login");
                return;
            }

            CurrentUserId = userId;

            // Load user profile with navigation properties
            var userWithNav = await UserManagerService.GetMyProfileAsync(CurrentUserId);

            if (userWithNav?.User == null)
            {
                await ToastService.Error("Lỗi", "Không tìm thấy thông tin người dùng");
                return;
            }

            // Set read-only display fields
            FullName = userWithNav.User.FullName ?? $"{userWithNav.User.FirstName} {userWithNav.User.LastName}";
            UserCode = userWithNav.User.UserCode ?? "";
            DisplayRole = userWithNav.RoleNames?.Any() == true ? string.Join(", ", userWithNav.RoleNames) : "";
            DisplayPosition = userWithNav.Position?.Name ?? "";
            DisplayTeam = userWithNav.Team?.Name ?? "";
            AvatarUrl = userWithNav.User.AvatarURL ?? "";
            Initials = GetInitials(userWithNav.User.FirstName, userWithNav.User.LastName);

            // Populate editable form model
            Model = new UpdateProfileDto
            {
                FirstName = userWithNav.User.FirstName ?? "",
                LastName = userWithNav.User.LastName ?? "",
                Gender = (int)userWithNav.User.Gender,
                DOB = userWithNav.User.DOB,
                PhoneNumber = userWithNav.User.PhoneNumber ?? "",
                Email = userWithNav.User.Email ?? "",
                Address = userWithNav.User.Address ?? "",
                AvatarURL = userWithNav.User.AvatarURL,
                IsSetPassword = false
            };

            // The profile itself is ready — paint it now. The authenticator card below and the
            // YubiKey card fetch on their own and re-render when they land. Holding the whole
            // page behind them meant one slow call left the user watching a bare spinner.
            IsLoadingData = false;
            StateHasChanged();

            await LoadTotpStatus();
            await LoadSecurityKeys();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải thông tin cá nhân: {ex.Message}");
        }
        finally
        {
            IsLoadingData = false;
        }
    }

    /// <summary>
    /// Xử lý submit form - cập nhật thông tin cá nhân
    /// </summary>
    private async Task OnValidSubmit(EditContext context)
    {
        if (Model == null) return;

        // Validate password match if changing password
        if (Model.IsSetPassword)
        {
            if (string.IsNullOrWhiteSpace(Model.Password))
            {
                await ToastService.Warning("Cảnh báo", "Vui lòng nhập mật khẩu mới");
                return;
            }
            if (Model.Password != Model.PasswordConfirm)
            {
                await ToastService.Warning("Cảnh báo", "Mật khẩu xác nhận không khớp");
                return;
            }
        }

        IsLoading = true;
        try
        {
            var result = await UserManagerService.UpdateMyProfileAsync(Model, CurrentUserId);

            if (result != null)
            {
                await ToastService.Success("Thành công", "Cập nhật thông tin cá nhân thành công!");

                // Reload profile to refresh display
                await LoadProfile();
            }
            else
            {
                await ToastService.Error("Lỗi", "Không thể cập nhật thông tin. Vui lòng thử lại.");
            }
        }
        catch (BootstrapBlazor.Server.Exceptions.BadRequestException ex)
        {
            await ToastService.Error("Lỗi Validation", ex.Message);
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Có lỗi xảy ra: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Reload profile data
    /// </summary>
    private async Task OnReload()
    {
        await LoadProfile();
    }

    private async Task LoadSecurityKeys()
    {
        if (WebAuthnApiService == null)
        {
            return;
        }

        IsLoadingSecurityKeys = true;
        try
        {
            var status = await WebAuthnApiService.GetFeatureStatusAsync();
            SecurityKeyFeatureEnabled = status?.Enabled == true;
            SecurityKeyEnforced = status?.Enforced == true;

            if (!SecurityKeyFeatureEnabled)
            {
                SecurityKeys = new List<SecurityKeyDto>();
                return;
            }

            SecurityKeys = await WebAuthnApiService.GetKeysAsync() ?? new List<SecurityKeyDto>();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"LoadSecurityKeys Error: {ex.Message}");
            SecurityKeyFeatureEnabled = false;
            SecurityKeys = new List<SecurityKeyDto>();
        }
        finally
        {
            IsLoadingSecurityKeys = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Called back from JS once the ceremony finishes. It runs in the browser, inside the click,
    /// because navigator.credentials ignores a request that has no user gesture behind it.
    /// </summary>
    [JSInvokable]
    public async Task OnWebAuthnCeremonyFinished(WebAuthnBrowserResult result)
    {
        IsSecurityKeyBusy = false;

        if (result.Ok)
        {
            await LoadSecurityKeys();
            if (ToastService != null)
            {
                await ToastService.Success("Thành công", "Đã đăng ký khoá bảo mật.");
            }
        }
        else if (ToastService != null)
        {
            await ToastService.Error("Không đăng ký được", DescribeCeremonyError(result));
        }

        StateHasChanged();
    }

    private static string DescribeCeremonyError(WebAuthnBrowserResult result) => result.Error switch
    {
        "NotAllowedError" => "Không nhận được thao tác chạm khoá. Thử lại.",
        "InvalidStateError" => "Khoá này đã được đăng ký. Dùng khoá khác.",
        "NotSupportedError" => "Thiết bị không hỗ trợ khoá bảo mật.",
        "SecurityError" => "Sai địa chỉ truy cập cho khoá bảo mật. Báo quản trị viên kiểm tra cấu hình.",
        "OperationError" => "Không tìm thấy khoá bảo mật nào trên máy này. Cắm khoá rồi thử lại.",
        "CeremonyTimeout" => "Trình duyệt không mở được hộp thoại khoá bảo mật. Thường do một tiện ích "
            + "quản lý mật khẩu đang chiếm quyền: thử lại trong cửa sổ ẩn danh, hoặc tắt tiện ích đó.",
        "ApiError" => string.IsNullOrWhiteSpace(result.Message) ? "Máy chủ từ chối." : result.Message!,
        _ => $"Không xác thực được khoá ({result.Error}). Thử lại."
    };

    private async Task BindWebAuthnButtonAsync()
    {
        if (WebAuthnBound || !SecurityKeyFeatureEnabled || JSRuntime == null)
        {
            return;
        }

        try
        {
            WebAuthnModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", WebAuthnModulePath.Value);
            WebAuthnRef ??= DotNetObjectReference.Create(this);
            WebAuthnBound = await WebAuthnModule.InvokeAsync<bool>(
                "bindCeremony", RegisterKeyButtonId, "register", null, WebAuthnRef);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[WebAuthn] could not bind the register button: {ex.Message}");
        }
    }

    private async Task OnDeleteSecurityKey(SecurityKeyDto key)
    {
        if (WebAuthnApiService == null || IsSecurityKeyBusy)
        {
            return;
        }

        IsSecurityKeyBusy = true;
        try
        {
            await WebAuthnApiService.DeleteKeyAsync(key.Id);
            await LoadSecurityKeys();
            await ToastService.Success("Thành công", $"Đã xoá khoá \"{key.DeviceName}\".");
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không xoá được khoá: {ex.Message}");
        }
        finally
        {
            IsSecurityKeyBusy = false;
            StateHasChanged();
        }
    }

    private async Task LoadTotpStatus()
    {
        if (TotpApiService == null)
        {
            return;
        }

        IsLoadingTotp = true;
        try
        {
            TotpFeatureEnabled = await TotpApiService.IsFeatureEnabledAsync();
            TotpStatusLoaded = true;
            if (!TotpFeatureEnabled)
            {
                TotpEnabled = false;
                RemainingRecoveryCodes = 0;
                return;
            }

            var status = await TotpApiService.GetStatusAsync();
            TotpEnabled = status?.TotpEnabled == true;
            RemainingRecoveryCodes = status?.RemainingRecoveryCodes ?? 0;
        }
        catch
        {
            TotpStatusLoaded = true;
            TotpFeatureEnabled = false;
            TotpEnabled = false;
            RemainingRecoveryCodes = 0;
        }
        finally
        {
            IsLoadingTotp = false;
            StateHasChanged();
        }
    }

    private async Task OnStartTotpSetup()
    {
        if (TotpApiService == null)
        {
            return;
        }

        IsTotpBusy = true;
        RecoveryCodes.Clear();
        try
        {
            TotpSetup = await TotpApiService.SetupAsync();
            TotpConfirmCode = "";
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tạo mã QR: {ex.Message}");
        }
        finally
        {
            IsTotpBusy = false;
        }
    }

    private async Task OnConfirmTotpSetup()
    {
        if (TotpApiService == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TotpConfirmCode))
        {
            await ToastService.Warning("Cảnh báo", "Vui lòng nhập mã Authenticator");
            return;
        }

        IsTotpBusy = true;
        try
        {
            var result = await TotpApiService.ConfirmAsync(TotpConfirmCode);
            RecoveryCodes = result?.RecoveryCodes ?? new List<string>();
            TotpSetup = null;
            TotpConfirmCode = "";
            await LoadTotpStatus();
            await ToastService.Success("Thành công", "Đã bật Google Authenticator");
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể xác nhận mã: {ex.Message}");
        }
        finally
        {
            IsTotpBusy = false;
        }
    }

    private async Task OnDisableTotp()
    {
        if (TotpApiService == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TotpActionCode))
        {
            await ToastService.Warning("Cảnh báo", "Vui lòng nhập mã Authenticator");
            return;
        }

        IsTotpBusy = true;
        try
        {
            await TotpApiService.DisableAsync(TotpActionCode);
            TotpActionCode = "";
            RecoveryCodes.Clear();
            await LoadTotpStatus();
            await ToastService.Success("Thành công", "Đã tắt Google Authenticator");
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tắt Authenticator: {ex.Message}");
        }
        finally
        {
            IsTotpBusy = false;
        }
    }

    private async Task OnRegenerateRecoveryCodes()
    {
        if (TotpApiService == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TotpActionCode))
        {
            await ToastService.Warning("Cảnh báo", "Vui lòng nhập mã Authenticator");
            return;
        }

        IsTotpBusy = true;
        try
        {
            var result = await TotpApiService.RegenerateRecoveryCodesAsync(TotpActionCode);
            RecoveryCodes = result?.RecoveryCodes ?? new List<string>();
            TotpActionCode = "";
            await LoadTotpStatus();
            await ToastService.Success("Thành công", "Đã tạo lại recovery codes");
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tạo lại recovery codes: {ex.Message}");
        }
        finally
        {
            IsTotpBusy = false;
        }
    }

    /// <summary>
    /// Tạo initials từ tên (e.g., "Nguyễn Văn" → "NV")
    /// </summary>
    private static string GetInitials(string? firstName, string? lastName)
    {
        var first = !string.IsNullOrEmpty(firstName) ? firstName.Trim()[0].ToString().ToUpper() : "";
        var last = !string.IsNullOrEmpty(lastName) ? lastName.Trim()[0].ToString().ToUpper() : "";

        var result = $"{first}{last}".Trim();
        return string.IsNullOrEmpty(result) ? "?" : result;
    }
}
