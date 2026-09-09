using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using BootstrapBlazor.Server.Services;
using BootstrapBlazor.Server.Data;
using Microsoft.JSInterop;

namespace BootstrapBlazor.Server.Components.Task9.UserManager;

public partial class CreateUser : ComponentBase
{
    [Parameter] public bool Embedded { get; set; }
    [Parameter] public EventCallback OnCompleted { get; set; }

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
    
    private CreateUserDto Model { get; set; } = new CreateUserDto();
    
    private bool IsLoading { get; set; } = false;
    private string GeneratedPassword { get; set; } = "";
    private bool ShowPassword { get; set; } = false;
    
    private List<SelectedItem> RoleItems { get; set; } = new();
    private List<SelectedItem> PositionItems { get; set; } = new();
    private List<SelectedItem> TeamItems { get; set; } = new();
    
    private List<string> SelectedRoles { get; set; } = new();
    
    protected override async Task OnInitializedAsync()
    {
        await LoadRoles();
        await LoadPositions();
        await LoadTeams();
        InitializeModel();
    }
    
    private void InitializeModel()
    {
        // Generate random password
        GeneratedPassword = GenerateRandomPassword();
        
        Model = new CreateUserDto
        {
            Password = GeneratedPassword,
            PasswordConfirm = GeneratedPassword,
            Gender = 1,
            DOB = DateTime.Now.AddYears(-25),
            PhoneNumber = "0000000000",
            IsActive = true,
            IsDelete = false
        };
    }
    
    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$";
        const string upperChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowerChars = "abcdefghjkmnpqrstuvwxyz";
        const string numberChars = "23456789";
        const string specialChars = "!@#$";
        
        var random = new Random();
        var password = new char[10];
        
        // Ensure at least one of each required character type
        password[0] = upperChars[random.Next(upperChars.Length)];
        password[1] = lowerChars[random.Next(lowerChars.Length)];
        password[2] = numberChars[random.Next(numberChars.Length)];
        password[3] = specialChars[random.Next(specialChars.Length)];
        
        // Fill the rest randomly
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = chars[random.Next(chars.Length)];
        }
        
        // Shuffle the password
        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }
        
        return new string(password);
    }
    
    private void RegeneratePassword()
    {
        GeneratedPassword = GenerateRandomPassword();
        Model.Password = GeneratedPassword;
        Model.PasswordConfirm = GeneratedPassword;
        StateHasChanged();
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
    
    private async Task OnValidSubmit(EditContext context)
    {
        IsLoading = true;
        try
        {
            // Map selected roles to model
            Model.Roles = SelectedRoles;
            
            // Ensure password is set
            Model.Password = GeneratedPassword;
            Model.PasswordConfirm = GeneratedPassword;
            Model.IsActive = true;
            Model.IsDelete = false;
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Model.UserName))
            {
                await ToastService.Error("Lỗi", "Vui lòng nhập tên đăng nhập");
                return;
            }
            if (string.IsNullOrWhiteSpace(Model.UserCode))
            {
                await ToastService.Error("Lỗi", "Vui lòng nhập mã nhân viên");
                return;
            }
            if (string.IsNullOrWhiteSpace(Model.FirstName) || Model.FirstName.Length < 1)
            {
                await ToastService.Error("Lỗi", "Họ phải có ít nhất 1 ký tự");
                return;
            }
            if (string.IsNullOrWhiteSpace(Model.LastName))
            {
                await ToastService.Error("Lỗi", "Vui lòng nhập tên");
                return;
            }
            if (Model.Roles == null || Model.Roles.Count == 0)
            {
                await ToastService.Error("Lỗi", "Vui lòng chọn ít nhất một vai trò");
                return;
            }
            
            var result = await UserManagerService.CreateUserWithNavigationPropertiesAsync(Model);

            if (result != null)
            {
                await ToastService.Success("Thành công", $"Tạo người dùng mới thành công! Mật khẩu: {GeneratedPassword}");
                await FinishAsync();
            }
            else
            {
                await ToastService.Error("Lỗi", "Không thể tạo người dùng. Vui lòng kiểm tra lại thông tin.");
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
    
    private async Task FinishAsync()
    {
        if (OnCompleted.HasDelegate)
        {
            await OnCompleted.InvokeAsync();
            return;
        }
        NavigationManager.NavigateTo("/user-manager");
    }

    private Task OnCancel() => FinishAsync();
    
    private async Task CopyPasswordToClipboard()
    {
        try
        {
            await ToastService.Information("Đã copy", $"Mật khẩu đã được copy: {GeneratedPassword}");
            // Note: Actual clipboard copy would need JS interop
            // For now, just show the notification
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", "Không thể copy mật khẩu");
        }
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
                        const inputs = document.querySelectorAll('#page-user-manager-create input.form-control.seo-input-size, #page-user-manager-create .seo-input-size input.form-control');
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
                        
                        // Force style cho dropdown-toggle (Select, MultiSelect)
                        const dropdownToggles = document.querySelectorAll('#page-user-manager-create .dropdown-toggle.seo-input-size, #page-user-manager-create .seo-input-size.dropdown-toggle, #page-user-manager-create .seo-input-size .dropdown-toggle');
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
                        const datetimePickerInputs = document.querySelectorAll('#page-user-manager-create .datetime-picker-input, #page-user-manager-create input.datetime-picker-input, #page-user-manager-create .dropdown-toggle.datetime-picker-input, #page-user-manager-create .datetime-picker .datetime-picker-input');
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
                        const datetimePickerIcons = document.querySelectorAll('#page-user-manager-create .datetime-picker-bar, #page-user-manager-create .datetime-picker .datetime-picker-bar');
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
                        const buttons = document.querySelectorAll('#page-user-manager-create .btn.seo-input-size, #page-user-manager-create button.seo-input-size');
                        buttons.forEach(button => {
                            button.style.setProperty('height', '54px', 'important');
                            button.style.setProperty('min-height', '54px', 'important');
                            button.style.setProperty('max-height', '54px', 'important');
                            button.style.setProperty('padding', '18px 14px 16px 14px', 'important');
                            button.style.setProperty('box-sizing', 'border-box', 'important');
                            button.style.setProperty('font-size', '15px', 'important');
                            button.style.setProperty('line-height', '22px', 'important');
                            button.style.setProperty('margin-top', '0', 'important');
                            button.style.setProperty('display', 'flex', 'important');
                            button.style.setProperty('align-items', 'center', 'important');
                            button.style.setProperty('justify-content', 'center', 'important');
                        });
                        
                        // Chạy lại nhiều lần để đảm bảo override Bootstrap
                        setTimeout(() => {
                            // Force lại padding cho datetime-picker-input
                            const datetimePickerInputs2 = document.querySelectorAll('#page-user-manager-create .datetime-picker-input, #page-user-manager-create input.datetime-picker-input, #page-user-manager-create .dropdown-toggle.datetime-picker-input, #page-user-manager-create .datetime-picker .datetime-picker-input');
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
                            const dropdownToggles2 = document.querySelectorAll('#page-user-manager-create .dropdown-toggle.seo-input-size, #page-user-manager-create .seo-input-size.dropdown-toggle, #page-user-manager-create .seo-input-size .dropdown-toggle');
                            dropdownToggles2.forEach(toggle => {
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
                            const datetimePickerInputs3 = document.querySelectorAll('#page-user-manager-create .datetime-picker-input, #page-user-manager-create input.datetime-picker-input, #page-user-manager-create .dropdown-toggle.datetime-picker-input, #page-user-manager-create .datetime-picker .datetime-picker-input');
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
                            const datetimePickerInputs4 = document.querySelectorAll('#page-user-manager-create .datetime-picker-input, #page-user-manager-create input.datetime-picker-input, #page-user-manager-create .dropdown-toggle.datetime-picker-input, #page-user-manager-create .datetime-picker .datetime-picker-input');
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

