using Microsoft.AspNetCore.Components;
using BootstrapBlazor.Server.Data;

namespace BootstrapBlazor.Server.Components.Task9.UserManager;

public partial class UserList : ComponentBase
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
    
    [Inject]
    [NotNull]
    private SwalService? SwalService { get; set; }

    [Inject]
    [NotNull]
    private Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider? AuthStateProvider { get; set; }

    /// <summary>Only SUPER_ADMIN sees the "login as user" action.</summary>
    private bool IsSuperAdmin { get; set; }

    private List<UserWithNavigationPropertiesDto> Users { get; set; } = new();
    private int TotalItems { get; set; } = 0;
    private bool IsLoading { get; set; } = false;
    
    private static IEnumerable<int> PageItemsSource => new int[] { 10, 20, 40, 80, 100 };
    
    private Table<UserWithNavigationPropertiesDto>? TableRef { get; set; }
    private Modal? _userModal;
    private int? _editUserId;
    private bool _editorOpen;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            IsLoading = true;
            if (AuthStateProvider is BootstrapBlazor.Server.Identity.ApiAuthenticationStateProvider apiProvider)
            {
                IsSuperAdmin = await apiProvider.IsSuperAdminAsync();
            }
            var users = await UserManagerService.GetListWithNavigationAsync();
            Users = users ?? new List<UserWithNavigationPropertiesDto>();
            TotalItems = Users.Count;
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách người dùng: {ex.Message}");
            Users = new List<UserWithNavigationPropertiesDto>();
            TotalItems = 0;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task<QueryData<UserWithNavigationPropertiesDto>> OnQueryAsync(QueryPageOptions options)
    {
        try
        {
            IsLoading = true;

            var allUsers = await UserManagerService.GetListWithNavigationAsync();

            // Ensure allUsers is not null
            if (allUsers == null)
            {
                allUsers = new List<UserWithNavigationPropertiesDto>();
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(options.SearchText))
            {
                allUsers = allUsers.Where(u =>
                    u != null && u.User != null && (
                        (u.User.UserCode?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.User.UserName?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.User.FirstName?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.User.LastName?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.Team?.Name?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.User.Email?.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                    )
                ).ToList();
            }

            TotalItems = allUsers.Count;

            // Apply pagination
            Users = allUsers
                .Skip((options.PageIndex - 1) * options.PageItems)
                .Take(options.PageItems)
                .ToList();

            return new QueryData<UserWithNavigationPropertiesDto>()
            {
                Items = Users,
                TotalCount = TotalItems
            };
        }
        catch (Exception ex)
        {
            await ToastService.Error("Lỗi", $"Không thể tải danh sách người dùng: {ex.Message}");
            return new QueryData<UserWithNavigationPropertiesDto>()
            {
                Items = new List<UserWithNavigationPropertiesDto>(),
                TotalCount = 0
            };
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task OnCreateClick()
    {
        _editUserId = null;
        _editorOpen = true;
        if (_userModal != null) await _userModal.Toggle();
    }

    private async Task OnEditClick(int id)
    {
        _editUserId = id;
        _editorOpen = true;
        if (_userModal != null) await _userModal.Toggle();
    }

    private async Task OnUserEditorClosedAsync()
    {
        _editorOpen = false;
        if (_userModal != null) await _userModal.Toggle();
        if (TableRef != null) await TableRef.QueryAsync();
    }

    /// <summary>SUPER_ADMIN may log in as any lower role (ADMIN included) but not another SUPER_ADMIN (API also blocks).</summary>
    private static bool CanImpersonate(UserWithNavigationPropertiesDto row)
    {
        var roles = row.Roles?.Where(r => r != null && !string.IsNullOrEmpty(r.Name)).Select(r => r.Name!.Trim().ToUpperInvariant()) ?? Enumerable.Empty<string>();
        return !roles.Any(r => r == "SUPER_ADMIN");
    }

    private async Task OnImpersonateClick(UserWithNavigationPropertiesDto row)
    {
        var name = row.User?.UserName ?? "user";
        var confirmed = await SwalService.ShowModal(new SwalOption
        {
            Category = SwalCategory.Question,
            Title = "Đăng nhập hộ",
            Content = $"Đăng nhập với tư cách \"{name}\"? Mọi thao tác sẽ được thực hiện dưới tài khoản này. Thoát để quay lại."
        });
        if (!confirmed)
        {
            return;
        }

        try
        {
            var ok = await UserManagerService.StartImpersonationAsync(row.User!.Id);
            if (!ok)
            {
                await ToastService.Error("Đăng nhập hộ", "Không thể bắt đầu phiên đăng nhập hộ.");
                return;
            }
            // Reload as the impersonated user — menu/permissions rebuild from the new token.
            NavigationManager.NavigateTo("/", forceLoad: true);
        }
        catch (Exception ex)
        {
            await ToastService.Error("Đăng nhập hộ", ex.Message);
        }
    }

    /// <summary>Two-letter initials for the avatar chip (last + first name, falls back to username).</summary>
    private static string GetInitials(UserDto? user)
    {
        if (user == null) return "?";
        char First(string? s) => string.IsNullOrWhiteSpace(s) ? '\0' : char.ToUpperInvariant(s.Trim()[0]);
        var a = First(user.LastName);
        var b = First(user.FirstName);
        var initials = $"{a}{b}".Replace("\0", "");
        if (!string.IsNullOrEmpty(initials)) return initials;
        return string.IsNullOrWhiteSpace(user.UserName) ? "?" : char.ToUpperInvariant(user.UserName.Trim()[0]).ToString();
    }

    /// <summary>Color chip class per role so roles read at a glance instead of a comma list.</summary>
    private static string GetRoleChipClass(string role)
    {
        var r = role.Trim().ToUpperInvariant();
        var variant = r switch
        {
            "SUPER_ADMIN" => "role-superadmin",
            "ADMIN" => "role-admin",
            "HEAD" => "role-head",
            "ASSISTANT" => "role-assistant",
            "QC" => "role-qc",
            "IC" => "role-ic",
            "HR" => "role-hr",
            _ when r.Contains("SEO") => "role-seo",
            _ => "role-default"
        };
        return $"role-chip {variant}";
    }

    private string GetGenderText(Gender gender)
    {
        return gender switch
        {
            Gender.Male => "Nam",
            Gender.Female => "Nữ",
            Gender.Unknown => "Không xác định",
            _ => "N/A"
        };
    }
    
    private string GetUserTypeText(UserType userType)
    {
        return userType switch
        {
            UserType.Inhouse => "Inhouse",
            UserType.Remote => "Remote",
            UserType.OSZ => "OS Z",
            UserType.OS => "OS NA",
            _ => "N/A"
        };
    }
}
