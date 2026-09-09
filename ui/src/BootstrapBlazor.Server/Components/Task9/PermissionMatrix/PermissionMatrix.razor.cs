using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Components.Task9;

/// <summary>
/// Admin matrix: Role × Module/Page permission checkboxes stored in RoleClaims
/// (ClaimType "permission"). Saving diffs via role/create-claim + role/delete-claim.
/// </summary>
public partial class PermissionMatrix
{
    private const string PermissionClaimType = "permission";
    private const string PermissionMatrixCode = "system.permission-matrix";

    [Inject, NotNull] private IRoleManagerService? RoleService { get; set; }
    [Inject, NotNull] private ToastService? Toast { get; set; }

    private List<RoleDto> Roles { get; set; } = new();
    private List<IGrouping<string, PermissionCatalogItem>> CatalogByModule { get; set; } = new();

    // (roleId, permissionCode) → granted
    private readonly Dictionary<(int RoleId, string Code), bool> _current = new();
    private readonly Dictionary<(int RoleId, string Code), bool> _original = new();

    private bool IsLoading { get; set; } = true;
    private bool IsBusy { get; set; }

    private bool HasChanges => ChangeCount > 0;
    private int ChangeCount => _current.Count(kv => _original.TryGetValue(kv.Key, out var orig) && orig != kv.Value);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await ReloadAsync();
        }
    }

    private async Task ReloadAsync()
    {
        IsLoading = true;
        StateHasChanged();
        try
        {
            Roles = await RoleService.GetListAsync() ?? new List<RoleDto>();
            // SUPER_ADMIN bypasses all permission checks — hide it from the matrix.
            Roles = Roles.Where(r => r.Name?.Trim().ToUpperInvariant() != "SUPER_ADMIN").ToList();
            var catalog = await RoleService.GetPermissionCatalogAsync() ?? new List<PermissionCatalogItem>();
            CatalogByModule = catalog.GroupBy(c => c.ModuleText).ToList();

            _current.Clear();
            _original.Clear();
            foreach (var role in Roles)
            {
                var claims = await RoleService.GetClaimListAsync(role.Id) ?? new List<RoleClaimDto>();
                var granted = claims
                    .Where(c => c.ClaimType == PermissionClaimType && !string.IsNullOrEmpty(c.ClaimValue))
                    .Select(c => c.ClaimValue)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var item in catalog)
                {
                    var granted0 = granted.Contains(item.Code);
                    _current[(role.Id, item.Code)] = granted0;
                    _original[(role.Id, item.Code)] = granted0;
                }
            }
        }
        catch (Exception ex)
        {
            await Toast.Error("Phân quyền", $"Không tải được dữ liệu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private bool IsChecked(int roleId, string code)
        => _current.TryGetValue((roleId, code), out var v) && v;

    private bool IsDirty(int roleId, string code)
        => _current.TryGetValue((roleId, code), out var cur)
           && _original.TryGetValue((roleId, code), out var orig)
           && cur != orig;

    private void Toggle(int roleId, string code, bool value)
    {
        // Self-lock guard: never allow removing the permission-matrix page from ADMIN.
        if (!value && IsAdminRole(roleId) && code.Equals(PermissionMatrixCode, StringComparison.OrdinalIgnoreCase))
        {
            _ = Toast.Warning("Phân quyền", "Không thể thu quyền 'Phân quyền' của role ADMIN (tránh tự khóa).");
            return;
        }

        _current[(roleId, code)] = value;
    }

    private void ToggleModule(int roleId, string moduleText, bool value)
    {
        var group = CatalogByModule.FirstOrDefault(g => g.Key == moduleText);
        if (group == null) return;
        foreach (var item in group)
        {
            Toggle(roleId, item.Code, value);
        }
    }

    private bool IsAdminRole(int roleId)
        => Roles.FirstOrDefault(r => r.Id == roleId)?.Name?.Trim().ToUpperInvariant() == "ADMIN";

    private async Task SaveAsync()
    {
        var changes = _current
            .Where(kv => _original.TryGetValue(kv.Key, out var orig) && orig != kv.Value)
            .ToList();
        if (changes.Count == 0) return;

        IsBusy = true;
        StateHasChanged();
        var applied = 0;
        var failed = 0;
        try
        {
            foreach (var change in changes)
            {
                try
                {
                    if (change.Value)
                    {
                        await RoleService.CreateClaimAsync(change.Key.RoleId, PermissionClaimType, change.Key.Code);
                    }
                    else
                    {
                        await RoleService.DeleteClaimAsync(change.Key.RoleId, PermissionClaimType, change.Key.Code);
                    }
                    _original[change.Key] = change.Value;
                    applied++;
                }
                catch
                {
                    failed++;
                }
            }

            if (failed == 0)
            {
                await Toast.Success("Phân quyền",
                    $"Đã lưu {applied} thay đổi. API áp dụng trong ≤5 phút; menu của user cập nhật khi đăng nhập lại.");
            }
            else
            {
                await Toast.Warning("Phân quyền", $"Đã lưu {applied} thay đổi, {failed} lỗi — bấm Tải lại để kiểm tra.");
            }
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }
}
