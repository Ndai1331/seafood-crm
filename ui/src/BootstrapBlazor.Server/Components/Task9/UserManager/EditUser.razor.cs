using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using BootstrapBlazor.Server.Services;
using BootstrapBlazor.Server.Data;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Components.Task9.UserManager;

public partial class EditUser : ComponentBase
{
    [Parameter]
    public int UserId { get; set; }
    
    [Inject]
    private Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider? AuthStateProvider { get; set; }

    [Inject]
    [NotNull]
    private IUserManagerService? UserManagerService { get; set; }
    
    [Inject]
    [NotNull]
    private IRoleManagerService? RoleManagerService { get; set; }
    
    [Inject]
    [NotNull]
    private IPositionService? PositionService { get; set; }
    
    [Inject]
    [NotNull]
    private ITeamService? TeamService { get; set; }
    
    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }
    
    [Inject]
    [NotNull]
    private NavigationManager? NavigationManager { get; set; }
    
    [Inject]
    private IJSRuntime? JSRuntime { get; set; }

    [Inject]
    private ITotpApiService? TotpApiService { get; set; }

    [Inject]
    private IWebAuthnApiService? WebAuthnApiService { get; set; }

    [Inject]
    private IYubikeyApiService? YubikeyApiService { get; set; }

    [Inject]
    private SwalService SwalService { get; set; } = default!;
    
    private UpdateUserDto? Model { get; set; }
    private bool IsLoading { get; set; } = false;
    private bool IsLoadingData { get; set; } = false;
    
    private List<SelectedItem> RoleItems { get; set; } = new();
    private List<SelectedItem> PositionItems { get; set; } = new();
    private List<SelectedItem> TeamItems { get; set; } = new();
    
    private List<string> SelectedRoles { get; set; } = new();
    private int? SelectedPositionId { get; set; }
    private int? SelectedTeamId { get; set; }

    private bool AdminTotpFeatureLoaded { get; set; }
    private bool AdminTotpFeatureEnabled { get; set; }
    private bool AdminTotpEnabled { get; set; }
    private bool IsAdminTotpBusy { get; set; }

    private bool AdminSecurityKeyFeatureEnabled { get; set; }
    private int AdminSecurityKeyCount { get; set; }
    private DateTime? AdminSecurityKeyLastUsed { get; set; }
    private bool IsAdminSecurityKeyBusy { get; set; }
    private int AdminRemainingRecoveryCodes { get; set; }
    private List<string> AdminRecoveryCodes { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    private async Task LoadData()
    {
        IsLoadingData = true;
        try
        {
            await LoadRoles();
            await LoadPositions();
            await LoadTeams();
            await LoadUser();
            await LoadAdminTotpStatus();
            await LoadAdminSecurityKeyStatus();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải dữ liệu: {ex.Message}");
        }
        finally
        {
            IsLoadingData = false;
        }
    }
    
    private async Task LoadUser()
    {
        try
        {
            var userWithNav = await UserManagerService.GetWithNavigationProperties(UserId);
            
            if (userWithNav == null || userWithNav.User == null)
            {
                await ToastService.Error("Lỗi", "Không tìm thấy thông tin người dùng");
                return;
            }
            
            Model = new UpdateUserDto
            {
                UserName = userWithNav.User.UserName,
                UserCode = userWithNav.User.UserCode,
                FirstName = userWithNav.User.FirstName,
                LastName = userWithNav.User.LastName,
                Email = userWithNav.User.Email ?? "",
                PhoneNumber = userWithNav.User.PhoneNumber,
                Gender = (int)userWithNav.User.Gender, // Cast enum to int
                UserType = (int)userWithNav.User.UserType, // Cast enum to int
                DOB = userWithNav.User.DOB,
                IsActive = userWithNav.User.IsActive,
                PositionId = userWithNav.Position?.Id,
                TeamId = userWithNav.Team?.Id,
                Address = userWithNav.User.Address,
                Roles = userWithNav.RoleNames,
                IsSetPassword = false
            };
            
            SelectedRoles = userWithNav.RoleNames;
            SelectedPositionId = userWithNav.Position?.Id;
            SelectedTeamId = userWithNav.Team?.Id;
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải thông tin người dùng: {ex.Message}");
        }
    }
    
    private async Task LoadRoles()
    {
        try
        {
            // API có thể trả null khi token chưa attach lúc trang vừa load (race 401)
            var roles = await RoleManagerService.GetListAsync();
            if (roles == null)
            {
                await ToastService.Warning("Chú ý", "Không tải được danh sách vai trò. Vui lòng tải lại trang.");
                return;
            }
            // SUPER_ADMIN is only assignable by a SUPER_ADMIN (API enforces the same rule).
            var isSuperAdmin = false;
            if (AuthStateProvider is BootstrapBlazor.Server.Identity.ApiAuthenticationStateProvider apiProvider)
            {
                // Check ALL role claims — users can be ADMIN + SUPER_ADMIN at once.
                isSuperAdmin = await apiProvider.IsSuperAdminAsync();
            }
            RoleItems = roles
                .Where(r => isSuperAdmin || r.Name != "SUPER_ADMIN")
                .Select(r => new SelectedItem(r.Name, r.Name)).ToList();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách vai trò: {ex.Message}");
        }
    }
    
    private async Task LoadPositions()
    {
        try
        {
            var positions = await PositionService.GetListAsync();
            if (positions == null)
            {
                await ToastService.Warning("Chú ý", "Không tải được danh sách chức vụ. Vui lòng tải lại trang.");
                return;
            }
            PositionItems = positions.Select(p => new SelectedItem(p.Id.ToString(), p.Name)).ToList();
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách chức vụ: {ex.Message}");
        }
    }
    
    private async Task LoadTeams()
    {
        try
        {
            if (TeamService != null)
            {
                var filter = new BaseFilterPagingDto
                {
                    Skip = 0,
                    Take = 1000,
                    FilterText = string.Empty
                };
                var response = await TeamService.GetListAsync(filter);
                if (response?.Status == true && response.Data != null)
                {
                    TeamItems = response.Data.Select(t => new SelectedItem(t.Id.ToString(), t.Name)).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách team: {ex.Message}");
        }
    }

    private async Task LoadAdminSecurityKeyStatus()
    {
        if (YubikeyApiService == null)
        {
            return;
        }

        try
        {
            var feature = await YubikeyApiService.GetFeatureStatusAsync();
            AdminSecurityKeyFeatureEnabled = feature?.Enabled == true;
            if (!AdminSecurityKeyFeatureEnabled)
            {
                return;
            }

            var status = await YubikeyApiService.AdminGetStatusAsync(UserId);
            AdminSecurityKeyCount = status?.KeyCount ?? 0;
            AdminSecurityKeyLastUsed = status?.LastUsedAt;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"LoadAdminSecurityKeyStatus Error: {ex.Message}");
            AdminSecurityKeyFeatureEnabled = false;
        }
    }

    private async Task OnAdminResetSecurityKeys()
    {
        if (WebAuthnApiService == null || IsAdminSecurityKeyBusy)
        {
            return;
        }

        // Spelled out because both halves surprise people: the user has to enrol again, and any
        // session they (or whoever took the account) currently hold is dropped.
        var confirmed = await SwalService.ShowModal(new SwalOption
        {
            Category = SwalCategory.Question,
            Title = "Gỡ khoá bảo mật của user này?",
            Content = "User sẽ phải đăng ký khoá mới ở lần đăng nhập tới, và mọi phiên đang mở "
                + "của user này sẽ bị đăng xuất. Tiếp tục?",
            ShowClose = true,
            ShowFooter = true
        });

        if (!confirmed)
        {
            return;
        }

        IsAdminSecurityKeyBusy = true;
        try
        {
            await YubikeyApiService!.AdminResetAsync(UserId);
            await LoadAdminSecurityKeyStatus();
            await ToastService.Success("Thành công", "Đã gỡ toàn bộ khoá bảo mật của user.");
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không gỡ được khoá: {ex.Message}");
        }
        finally
        {
            IsAdminSecurityKeyBusy = false;
            StateHasChanged();
        }
    }

    private async Task LoadAdminTotpStatus()
    {
        if (TotpApiService == null)
        {
            return;
        }

        try
        {
            AdminTotpFeatureEnabled = await TotpApiService.IsFeatureEnabledAsync();
            AdminTotpFeatureLoaded = true;
            if (!AdminTotpFeatureEnabled)
            {
                AdminTotpEnabled = false;
                AdminRemainingRecoveryCodes = 0;
                return;
            }

            var status = await TotpApiService.AdminGetStatusAsync(UserId);
            AdminTotpEnabled = status?.TotpEnabled == true;
            AdminRemainingRecoveryCodes = status?.RemainingRecoveryCodes ?? 0;
        }
        catch (Exception ex)
        {
            AdminTotpFeatureLoaded = true;
            AdminTotpFeatureEnabled = false;
            await ToastService.Error("Lỗi", $"Không thể tải trạng thái Authenticator: {ex.Message}");
        }
    }

    private async Task OnAdminDisableTotp()
    {
        if (TotpApiService == null)
        {
            return;
        }

        IsAdminTotpBusy = true;
        try
        {
            await TotpApiService.AdminDisableAsync(UserId);
            AdminRecoveryCodes.Clear();
            await LoadAdminTotpStatus();
            await ToastService.Success("Thành công", "Đã tắt 2FA của user");
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tắt 2FA: {ex.Message}");
        }
        finally
        {
            IsAdminTotpBusy = false;
        }
    }

    private async Task OnAdminRegenerateRecoveryCodes()
    {
        if (TotpApiService == null)
        {
            return;
        }

        IsAdminTotpBusy = true;
        try
        {
            var result = await TotpApiService.AdminRegenerateRecoveryCodesAsync(UserId);
            AdminRecoveryCodes = result?.RecoveryCodes ?? new List<string>();
            await LoadAdminTotpStatus();
            await ToastService.Success("Thành công", "Đã tạo lại recovery codes");
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tạo lại recovery codes: {ex.Message}");
        }
        finally
        {
            IsAdminTotpBusy = false;
        }
    }
    
    private async Task OnCloneUser()
    {
        IsLoading = true;
        try
        {
            var result = await UserManagerService.CloneUserAsync(UserId);
            if (result != null)
            {
                await ToastService.Success("Thành công", "Nhân bản người dùng thành công!");
                NavigationManager.NavigateTo($"/user-manager/edit/{result.Id}");
            }
            else
            {
                await ToastService.Error("Lỗi", "Không thể nhân bản người dùng. Vui lòng kiểm tra lại thông tin.");
            }
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể nhân bản người dùng: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task OnValidSubmit(EditContext context)
    {
        if (Model == null) return;
        
        IsLoading = true;
        try
        {
            // Map selected roles, position and team to model
            Model.Roles = SelectedRoles;
            Model.PositionId = SelectedPositionId;
            Model.TeamId = SelectedTeamId;
            
            var result = await UserManagerService.UpdateUserWithNavigationPropertiesAsync(Model, UserId);
            
            if (result != null)
            {
                await ToastService.Success("Thành công", "Cập nhật người dùng thành công!");
                
                // Navigate back to list
                NavigationManager.NavigateTo("/user-manager");
            }
            else
            {
                await ToastService.Error("Lỗi", "Không thể cập nhật người dùng. Vui lòng kiểm tra lại thông tin.");
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
    
    private void OnCancel()
    {
        NavigationManager.NavigateTo("/user-manager");
    }
    
    private void OnBackToList()
    {
        NavigationManager.NavigateTo("/user-manager");
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        // Force style cho tất cả input để đảm bảo cùng chiều cao
        if (JSRuntime != null)
        {
            await Task.Delay(50);
            try
            {
                var jsCode = @"
                    (function() {
                        // Force style cho input form-control với seo-input-size
                        const inputs = document.querySelectorAll('#page-user-manager-edit input.form-control.seo-input-size, #page-user-manager-edit .seo-input-size input.form-control');
                        inputs.forEach(input => {
                            input.style.setProperty('height', '54px', 'important');
                            input.style.setProperty('min-height', '54px', 'important');
                            input.style.setProperty('max-height', '54px', 'important');
                            input.style.setProperty('padding', '18px 14px 16px 14px', 'important');
                            input.style.setProperty('box-sizing', 'border-box', 'important');
                            input.style.setProperty('font-size', '15px', 'important');
                            input.style.setProperty('line-height', '22px', 'important');
                            input.style.setProperty('margin-top', '0', 'important');
                            input.style.setProperty('border', '0.5px solid rgba(145, 158, 171, 0.2)', 'important');
                            input.style.setProperty('border-radius', '10px', 'important');
                        });
                        
                        // Force style cho dropdown-toggle (Select, MultiSelect)
                        const dropdownToggles = document.querySelectorAll('#page-user-manager-edit .dropdown-toggle.seo-input-size, #page-user-manager-edit .seo-input-size.dropdown-toggle, #page-user-manager-edit .seo-input-size .dropdown-toggle');
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
                            toggle.style.setProperty('overflow', 'visible', 'important'); // Đổi từ hidden sang visible để text không bị che
                            toggle.style.setProperty('overflow-x', 'hidden', 'important'); // Chỉ ẩn overflow ngang
                            toggle.style.setProperty('overflow-y', 'visible', 'important'); // Visible để tag không bị cắt
                            toggle.style.setProperty('border', '0.5px solid rgba(145, 158, 171, 0.2)', 'important');
                            toggle.style.setProperty('border-radius', '10px', 'important');
                            
                            // Styling cho multi-select-items - có gap giữa các tag, padding-top để tag không bị che
                            const multiSelectItems = toggle.querySelectorAll('.multi-select-items');
                            multiSelectItems.forEach(item => {
                                item.style.setProperty('display', 'flex', 'important');
                                item.style.setProperty('align-items', 'center', 'important');
                                item.style.setProperty('flex-wrap', 'wrap', 'important');
                                item.style.setProperty('gap', '4px', 'important'); // Khoảng cách nhỏ giữa các tag
                                item.style.setProperty('overflow', 'visible', 'important');
                                item.style.setProperty('max-width', '100%', 'important');
                                item.style.setProperty('position', 'relative', 'important');
                                item.style.setProperty('z-index', '1', 'important');
                                item.style.setProperty('min-height', '22px', 'important'); // Đảm bảo có đủ chiều cao cho tag
                            });
                            
                            // Styling cho multi-select-item - background xanh, border-radius, padding
                            const multiSelectItemElements = toggle.querySelectorAll('.multi-select-item:not(.multi-select-item-group .multi-select-item)');
                            multiSelectItemElements.forEach(item => {
                                item.style.setProperty('background-color', '#007AFF', 'important');
                                item.style.setProperty('border-radius', '20px', 'important');
                                item.style.setProperty('padding', '4px 8px', 'important');
                                item.style.setProperty('margin', '0', 'important');
                                item.style.setProperty('border', 'none', 'important');
                                item.style.setProperty('font-size', '13px', 'important');
                                item.style.setProperty('font-weight', '500', 'important');
                                item.style.setProperty('color', '#FFFFFF', 'important');
                                item.style.setProperty('position', 'relative', 'important');
                                item.style.setProperty('z-index', '2', 'important');
                            });
                            
                            // Styling cho multi-select-item-group - background xanh, border-radius, padding
                            const multiSelectItemGroups = toggle.querySelectorAll('.multi-select-item-group');
                            multiSelectItemGroups.forEach(group => {
                                group.style.setProperty('gap', '0', 'important'); // Không có gap giữa text và icon X
                                group.style.setProperty('background-color', '#007AFF', 'important');
                                group.style.setProperty('border-radius', '20px', 'important');
                                group.style.setProperty('padding', '4px 8px', 'important');
                                group.style.setProperty('margin', '0', 'important');
                                group.style.setProperty('border', 'none', 'important');
                                group.style.setProperty('position', 'relative', 'important');
                                group.style.setProperty('z-index', '2', 'important');
                                
                                // Text bên trong group - không có background riêng, không có border
                                const itemText = group.querySelector('.multi-select-item');
                                if (itemText) {
                                    // Remove border properties trước
                                    itemText.style.removeProperty('border');
                                    itemText.style.removeProperty('border-width');
                                    itemText.style.removeProperty('border-style');
                                    itemText.style.removeProperty('border-color');
                                    
                                    // Set properties
                                    itemText.style.setProperty('background-color', 'transparent', 'important');
                                    itemText.style.setProperty('padding', '0', 'important');
                                    itemText.style.setProperty('margin', '0', 'important');
                                    itemText.style.setProperty('border-radius', '0', 'important');
                                    itemText.style.setProperty('font-size', '13px', 'important');
                                    itemText.style.setProperty('font-weight', '500', 'important');
                                    itemText.style.setProperty('color', '#FFFFFF', 'important');
                                    itemText.style.setProperty('border', 'none', 'important');
                                    itemText.style.setProperty('border-width', '0', 'important');
                                    itemText.style.setProperty('border-style', 'none', 'important');
                                    itemText.style.setProperty('border-color', 'transparent', 'important');
                                }
                            });
                            
                            // Styling cho multi-select-close - icon X nhỏ, màu đậm
                            const multiSelectCloses = toggle.querySelectorAll('.multi-select-close');
                            multiSelectCloses.forEach(close => {
                                // Remove tất cả properties trước
                                close.style.removeProperty('border');
                                close.style.removeProperty('margin');
                                close.style.removeProperty('margin-left');
                                close.style.removeProperty('padding');
                                
                                // Set properties với !important
                                close.style.setProperty('border', 'none', 'important');
                                close.style.setProperty('margin', '0', 'important');
                                close.style.setProperty('margin-left', '4px', 'important'); // Khoảng cách nhỏ giữa text và icon X
                                close.style.setProperty('padding', '0', 'important');
                                close.style.setProperty('width', '14px', 'important');
                                close.style.setProperty('height', '14px', 'important');
                                close.style.setProperty('min-width', '14px', 'important');
                                close.style.setProperty('min-height', '14px', 'important');
                                close.style.setProperty('max-width', '14px', 'important');
                                close.style.setProperty('max-height', '14px', 'important');
                                close.style.setProperty('background-color', 'transparent', 'important');
                                close.style.setProperty('cursor', 'pointer', 'important');
                                
                                // Icon X bên trong - xóa padding nhiều lần để override inline styles
                                const icons = close.querySelectorAll('i');
                                icons.forEach(icon => {
                                    // Remove tất cả padding properties nhiều lần
                                    icon.style.removeProperty('padding');
                                    icon.style.removeProperty('padding-top');
                                    icon.style.removeProperty('padding-right');
                                    icon.style.removeProperty('padding-bottom');
                                    icon.style.removeProperty('padding-left');
                                    icon.style.removeProperty('margin');
                                    
                                    // Set properties với !important
                                    icon.style.setProperty('font-size', '12px', 'important');
                                    icon.style.setProperty('line-height', '1', 'important');
                                    icon.style.setProperty('color', '#FFFFFF', 'important');
                                    icon.style.setProperty('display', 'flex', 'important');
                                    icon.style.setProperty('align-items', 'center', 'important');
                                    icon.style.setProperty('justify-content', 'center', 'important');
                                    icon.style.setProperty('padding', '0', 'important');
                                    icon.style.setProperty('padding-top', '0', 'important');
                                    icon.style.setProperty('padding-right', '0', 'important');
                                    icon.style.setProperty('padding-bottom', '0', 'important');
                                    icon.style.setProperty('padding-left', '0', 'important');
                                    icon.style.setProperty('margin', '0', 'important');
                                });
                            });
                            
                            // Xóa border của input bên trong dropdown-toggle - tìm tất cả input/form-control/form-select
                            const innerInputs = toggle.querySelectorAll('input, .form-control, .form-select, input.form-control, input.form-select, .form-select.form-control');
                            innerInputs.forEach(innerInput => {
                                // Remove border properties trước
                                innerInput.style.removeProperty('border');
                                innerInput.style.removeProperty('border-width');
                                innerInput.style.removeProperty('border-style');
                                innerInput.style.removeProperty('border-color');
                                
                                // Set border none
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
                        
                        // Force style cho datetime-picker-input - đảm bảo padding-left 40px
                        const datetimePickerInputs = document.querySelectorAll('#page-user-manager-edit .datetime-picker-input, #page-user-manager-edit input.datetime-picker-input, #page-user-manager-edit .dropdown-toggle.datetime-picker-input, #page-user-manager-edit .datetime-picker .datetime-picker-input');
                        datetimePickerInputs.forEach(input => {
                            // Remove padding trước để đảm bảo rewrite
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
                        
                        // Force style cho datetime-picker-bar (icon)
                        const datetimePickerIcons = document.querySelectorAll('#page-user-manager-edit .datetime-picker-bar, #page-user-manager-edit .datetime-picker .datetime-picker-bar');
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
                        
                        // Force style cho button seo-input-size
                        const buttons = document.querySelectorAll('#page-user-manager-edit .btn.seo-input-size, #page-user-manager-edit button.seo-input-size');
                        buttons.forEach(button => {
                            button.style.setProperty('height', '54px', 'important');
                            button.style.setProperty('min-height', '54px', 'important');
                            button.style.setProperty('max-height', '54px', 'important');
                            button.style.setProperty('padding', '18px 14px 16px 14px', 'important');
                            button.style.setProperty('box-sizing', 'border-box', 'important');
                            button.style.setProperty('font-size', '15px', 'important');
                            button.style.setProperty('line-height', '22px', 'important');
                            button.style.setProperty('margin-top', '0', 'important');
                            button.style.setProperty('margin-bottom', '0', 'important');
                            button.style.setProperty('display', 'flex', 'important');
                            button.style.setProperty('align-items', 'center', 'important');
                            button.style.setProperty('justify-content', 'center', 'important');
                        });
                        
                        // Force style cho button container để căn thẳng hàng
                        const buttonContainers = document.querySelectorAll('#page-user-manager-edit .col-12 .d-flex.gap-2');
                        buttonContainers.forEach(container => {
                            container.style.setProperty('align-items', 'center', 'important');
                            container.style.setProperty('margin-top', '0', 'important');
                        });
                        
                        // Chạy lại nhiều lần để đảm bảo override Bootstrap
                        setTimeout(() => {
                            // Force lại padding cho datetime-picker-input
                            const datetimePickerInputs2 = document.querySelectorAll('#page-user-manager-edit .datetime-picker-input, #page-user-manager-edit input.datetime-picker-input, #page-user-manager-edit .dropdown-toggle.datetime-picker-input, #page-user-manager-edit .datetime-picker .datetime-picker-input');
                            datetimePickerInputs2.forEach(input => {
                                // Remove padding trước
                                input.style.removeProperty('padding');
                                input.style.removeProperty('padding-top');
                                input.style.removeProperty('padding-right');
                                input.style.removeProperty('padding-bottom');
                                input.style.removeProperty('padding-left');
                                
                                // Set lại padding
                                input.style.setProperty('padding-top', '18px', 'important');
                                input.style.setProperty('padding-right', '14px', 'important');
                                input.style.setProperty('padding-bottom', '16px', 'important');
                                input.style.setProperty('padding-left', '40px', 'important');
                                input.style.setProperty('height', '54px', 'important');
                                input.style.setProperty('min-height', '54px', 'important');
                                input.style.setProperty('max-height', '54px', 'important');
                            });
                            
                            // Xóa border của input bên trong dropdown-toggle (chạy lại)
                            const dropdownToggles2 = document.querySelectorAll('#page-user-manager-edit .dropdown-toggle.seo-input-size, #page-user-manager-edit .seo-input-size.dropdown-toggle, #page-user-manager-edit .seo-input-size .dropdown-toggle');
                            dropdownToggles2.forEach(toggle => {
                                // Đảm bảo overflow visible để text không bị che
                                toggle.style.setProperty('overflow', 'visible', 'important');
                                toggle.style.setProperty('overflow-x', 'hidden', 'important');
                                toggle.style.setProperty('overflow-y', 'hidden', 'important');
                                
                                // Styling cho multi-select-items - có gap giữa các tag, padding-top để tag không bị che (chạy lại)
                                const multiSelectItems2 = toggle.querySelectorAll('.multi-select-items');
                                multiSelectItems2.forEach(item => {
                                    item.style.setProperty('gap', '4px', 'important'); // Khoảng cách nhỏ giữa các tag
                                    item.style.setProperty('padding-top', '0px', 'important'); // Padding-top để tag không bị che
                                    item.style.setProperty('overflow', 'visible', 'important');
                                    item.style.setProperty('min-height', '22px', 'important');
                                });
                              
                                
                                // Đảm bảo dropdown-toggle không cắt tag
                                const dropdownToggles2 = document.querySelectorAll('#page-user-manager-edit .dropdown-toggle.seo-input-size, #page-user-manager-edit .seo-input-size.dropdown-toggle, #page-user-manager-edit .seo-input-size .dropdown-toggle');
                                dropdownToggles2.forEach(toggle => {
                                    toggle.style.setProperty('overflow', 'visible', 'important');
                                    toggle.style.setProperty('overflow-x', 'hidden', 'important');
                                    toggle.style.setProperty('overflow-y', 'visible', 'important');
                                });
                                
                                // Styling cho multi-select-item - background xanh, border-radius, padding (chạy lại)
                                const multiSelectItemElements2 = toggle.querySelectorAll('.multi-select-item:not(.multi-select-item-group .multi-select-item, .multi-select-item-group .multi-select-item span)');
                                multiSelectItemElements2.forEach(item => {
                                    item.style.setProperty('background-color', '#007AFF', 'important');
                                    item.style.setProperty('border-radius', '20px', 'important');
                                    item.style.setProperty('padding', '4px 8px', 'important');
                                    item.style.setProperty('font-size', '13px', 'important');
                                    item.style.setProperty('font-weight', '500', 'important');
                                    item.style.setProperty('color', '#FFFFFF', 'important');
                                    item.style.setProperty('border', 'none', 'important');
                                });
                                
                                // Styling cho multi-select-item-group - background xanh, border-radius, padding (chạy lại)
                                const multiSelectItemGroups2 = toggle.querySelectorAll('.multi-select-item-group');
                                multiSelectItemGroups2.forEach(group => {
                                    group.style.setProperty('background-color', '#007AFF', 'important');
                                    group.style.setProperty('border-radius', '20px', 'important');
                                    group.style.setProperty('padding', '4px 8px', 'important');
                                    
                                    // Text bên trong group - xóa border
                                    const itemText2 = group.querySelector('.multi-select-item, .multi-select-item-group .multi-select-item span');
                                    if (itemText2) {
                                        // Remove border properties trước
                                        itemText2.style.removeProperty('border');
                                        itemText2.style.removeProperty('border-width');
                                        itemText2.style.removeProperty('border-style');
                                        itemText2.style.removeProperty('border-color');
                                        
                                        // Set properties
                                        itemText2.style.setProperty('background-color', 'transparent', 'important');
                                        itemText2.style.setProperty('padding', '0', 'important');
                                        itemText2.style.setProperty('font-size', '13px', 'important');
                                        itemText2.style.setProperty('font-weight', '500', 'important');
                                        itemText2.style.setProperty('color', '#FFFFFF', 'important');
                                        itemText2.style.setProperty('border', 'none', 'important');
                                        itemText2.style.setProperty('border-width', '0', 'important');
                                        itemText2.style.setProperty('border-style', 'none', 'important');
                                        itemText2.style.setProperty('border-color', 'transparent', 'important');
                                    }
                                });
                                
                                // Styling cho multi-select-close - icon X nhỏ, màu đậm (chạy lại)
                                const multiSelectCloses2 = toggle.querySelectorAll('.multi-select-close');
                                multiSelectCloses2.forEach(close => {
                                    // Remove và set lại để override inline styles
                                    close.style.removeProperty('margin');
                                    close.style.removeProperty('margin-left');
                                    close.style.setProperty('margin', '0', 'important');
                                    close.style.setProperty('margin-left', '4px', 'important');
                                    close.style.setProperty('width', '14px', 'important');
                                    close.style.setProperty('height', '14px', 'important');
                                    
                                    const icons2 = close.querySelectorAll('i');
                                    icons2.forEach(icon => {
                                        // Remove padding nhiều lần
                                        icon.style.removeProperty('padding');
                                        icon.style.removeProperty('padding-top');
                                        icon.style.removeProperty('padding-right');
                                        icon.style.removeProperty('padding-bottom');
                                        icon.style.removeProperty('padding-left');
                                        
                                        // Set padding = 0 với !important
                                        icon.style.setProperty('padding', '0', 'important');
                                        icon.style.setProperty('padding-top', '0', 'important');
                                        icon.style.setProperty('padding-right', '0', 'important');
                                        icon.style.setProperty('padding-bottom', '0', 'important');
                                        icon.style.setProperty('padding-left', '0', 'important');
                                        icon.style.setProperty('font-size', '12px', 'important');
                                        icon.style.setProperty('color', '#FFFFFF', 'important');
                                        icon.style.setProperty('margin', '0', 'important');
                                    });
                                });
                                
                                const innerInputs2 = toggle.querySelectorAll('input, .form-control, .form-select, input.form-control, input.form-select, .form-select.form-control');
                                innerInputs2.forEach(innerInput => {
                                    // Remove border properties trước
                                    innerInput.style.removeProperty('border');
                                    innerInput.style.removeProperty('border-width');
                                    innerInput.style.removeProperty('border-style');
                                    innerInput.style.removeProperty('border-color');
                                    
                                    // Set border none
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
                        }, 100);
                        
                        // Chạy lại sau 300ms
                        setTimeout(() => {
                            const datetimePickerInputs3 = document.querySelectorAll('#page-user-manager-edit .datetime-picker-input, #page-user-manager-edit input.datetime-picker-input, #page-user-manager-edit .dropdown-toggle.datetime-picker-input, #page-user-manager-edit .datetime-picker .datetime-picker-input');
                            datetimePickerInputs3.forEach(input => {
                                input.style.removeProperty('padding');
                                input.style.setProperty('padding-top', '18px', 'important');
                                input.style.setProperty('padding-right', '14px', 'important');
                                input.style.setProperty('padding-bottom', '16px', 'important');
                                input.style.setProperty('padding-left', '40px', 'important');
                            });
                        }, 300);
                        
                        // Chạy lại sau 500ms
                        setTimeout(() => {
                            const datetimePickerInputs4 = document.querySelectorAll('#page-user-manager-edit .datetime-picker-input, #page-user-manager-edit input.datetime-picker-input, #page-user-manager-edit .dropdown-toggle.datetime-picker-input, #page-user-manager-edit .datetime-picker .datetime-picker-input');
                            datetimePickerInputs4.forEach(input => {
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

}
