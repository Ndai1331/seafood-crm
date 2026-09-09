using BootstrapBlazor.Components;
using BootstrapBlazor.Server.Services;
using Microsoft.AspNetCore.Components.Web;

namespace BootstrapBlazor.Server.Components.Task9;

/// <summary>
/// Admin drag-drop menu arranger: reorder modules, reorder pages within a module, move a page
/// to a different module. Save flattens the edited model to a full override snapshot (POST
/// api/menu-layout) — presentation order/grouping only, never touches permission codes.
/// </summary>
public partial class MenuArrangement
{
    [Inject, NotNull] private IMenuLayoutClientService? Client { get; set; }
    [Inject, NotNull] private IMenuLayoutProvider? Provider { get; set; }
    [Inject, NotNull] private IRoleManagerService? RoleService { get; set; }
    [Inject, NotNull] private ToastService? Toast { get; set; }

    private List<EditModule> Modules { get; set; } = new();
    private List<EditRoleDefaultPage> RoleDefaults { get; set; } = new();
    private IReadOnlyList<PageEntry> DefaultPageOptions { get; } = PageRegistry.AllEntries().Where(e => !e.Hidden).ToList();

    // Url -> label as written in code, before any admin rename.
    private static readonly Dictionary<string, string> DefaultTextByUrl = PageRegistry.AllEntries()
        .GroupBy(e => e.Url, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(g => g.Key, g => g.First().Text, StringComparer.OrdinalIgnoreCase);
    private bool IsLoading { get; set; } = true;
    private bool IsBusy { get; set; }
    private bool IsDirty { get; set; }

    private string NewModuleName { get; set; } = "";
    private string NewModuleIcon { get; set; } = "fa-solid fa-folder";

    // Drag state — set on dragstart, consumed on drop. Null when not dragging.
    private (string ModuleText, string Url)? _draggingPage;
    private string? _draggingModule;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        IsLoading = true;
        StateHasChanged();
        try
        {
            var overrides = await Client.GetAsync();
            var merged = MenuOverrideMerger.Apply(PageRegistry.Modules, overrides);
            var enabledMap = MenuOverrideMerger.GetPageEnabledMap(overrides);
            Modules = merged
                .Select(m => new EditModule
                {
                    Text = m.Text,
                    Icon = m.Icon,
                    Items = m.Items
                        .Select(e => new EditPage
                        {
                            Url = e.Url,
                            // e.Text already carries the saved rename (applied by the merger);
                            // the registry lookup recovers the original for the reset button.
                            Text = e.Text,
                            DefaultText = DefaultTextByUrl.TryGetValue(e.Url, out var original) ? original : e.Text,
                            Icon = e.Icon,
                            Enabled = MenuOverrideMerger.IsPageVisible(e.Url, e.Hidden, enabledMap),
                            IsHiddenByDefault = e.Hidden,
                        })
                        .ToList(),
                })
                .ToList();
            await ReloadRoleDefaultsAsync();
            IsDirty = false;
        }
        catch (Exception ex)
        {
            await Toast.Error("Sắp xếp menu", $"Không tải được dữ liệu: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task ReloadRoleDefaultsAsync()
    {
        RoleDefaults = (await RoleService.GetDefaultPagesAsync())
            .Select(role => new EditRoleDefaultPage
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                DefaultPageUrl = role.DefaultPageUrl
            })
            .ToList();
    }

    private async Task SaveRoleDefaultsAsync()
    {
        IsBusy = true;
        try
        {
            await RoleService.SaveDefaultPagesAsync(RoleDefaults.Select(role => new RoleDefaultPageDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                DefaultPageUrl = role.DefaultPageUrl
            }).ToList());
            await Toast.Success("Trang mặc định", "Đã lưu trang mặc định theo vai trò.");
        }
        catch (Exception ex)
        {
            await Toast.Error("Trang mặc định", $"Lưu thất bại: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }

    private void ToggleEnabled(EditPage page)
    {
        page.Enabled = !page.Enabled;
        IsDirty = true;
    }

    // Inline rename: only one card is in edit mode at a time, keyed by page Url.
    private string? RenamingUrl { get; set; }
    private string RenameDraft { get; set; } = "";

    // Matches menu_overrides.text_override — a longer label would be silently cut off by MySQL.
    private const int MaxMenuTextLength = 256;

    // The rename box is rendered on demand, so focus has to be moved after that render — without
    // it the caret stays on <body> and the Enter/Esc shortcuts below never reach the input.
    private ElementReference RenameInput { get; set; }
    private bool _focusRenameInput;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_focusRenameInput)
        {
            _focusRenameInput = false;
            await RenameInput.FocusAsync();
        }
    }

    private void StartRename(EditPage page)
    {
        RenamingUrl = page.Url;
        RenameDraft = page.Text;
        _focusRenameInput = true;
    }

    private void CancelRename()
    {
        RenamingUrl = null;
        RenameDraft = "";
    }

    // Enter commits, Esc drops the edit — same reflexes as renaming a file.
    private void OnRenameKeyDown(KeyboardEventArgs e, EditPage page)
    {
        if (e.Key == "Enter")
        {
            CommitRename(page);
        }
        else if (e.Key == "Escape")
        {
            CancelRename();
        }
    }

    private void CommitRename(EditPage page)
    {
        var name = RenameDraft.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            _ = Toast.Warning("Đổi tên menu", "Tên menu không được để trống.");
            return;
        }
        if (name.Length > MaxMenuTextLength)
        {
            _ = Toast.Warning("Đổi tên menu", $"Tên menu tối đa {MaxMenuTextLength} ký tự.");
            return;
        }

        if (!name.Equals(page.Text, StringComparison.Ordinal))
        {
            page.Text = name;
            IsDirty = true;
        }
        CancelRename();
    }

    /// <summary>Drop the rename and fall back to the label defined in PageRegistry.</summary>
    private void ResetRename(EditPage page)
    {
        if (!page.Text.Equals(page.DefaultText, StringComparison.Ordinal))
        {
            page.Text = page.DefaultText;
            IsDirty = true;
        }
        CancelRename();
    }

    private void AddModule()
    {
        var name = NewModuleName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            _ = Toast.Warning("Sắp xếp menu", "Nhập tên module trước khi thêm.");
            return;
        }
        if (Modules.Any(m => m.Text.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            _ = Toast.Warning("Sắp xếp menu", $"Module \"{name}\" đã tồn tại.");
            return;
        }

        var icon = string.IsNullOrWhiteSpace(NewModuleIcon) ? "fa-solid fa-folder" : NewModuleIcon.Trim();
        Modules.Add(new EditModule { Text = name, Icon = icon, Items = new List<EditPage>() });
        NewModuleName = "";
        NewModuleIcon = "fa-solid fa-folder";
        IsDirty = true;
    }

    // Only a module NOT in PageRegistry is truly removable — a default module that merely
    // looks empty (all its entries are Hidden, e.g. "Khác"/"ETL Management") always reappears
    // after reload since Hidden entries attach to their PageRegistry module regardless of any
    // saved override row, so offering delete on it there would be misleading.
    private bool IsCustomModule(EditModule module)
        => !PageRegistry.Modules.Any(m => m.Text.Equals(module.Text, StringComparison.OrdinalIgnoreCase));

    private void RemoveEmptyModule(EditModule module)
    {
        if (module.Items.Count > 0 || !IsCustomModule(module)) return;
        Modules.Remove(module);
        IsDirty = true;
    }

    private void MoveModule(int index, int delta)
    {
        var target = index + delta;
        if (target < 0 || target >= Modules.Count) return;
        (Modules[index], Modules[target]) = (Modules[target], Modules[index]);
        IsDirty = true;
    }

    private void MovePage(EditModule module, int index, int delta)
    {
        var target = index + delta;
        if (target < 0 || target >= module.Items.Count) return;
        (module.Items[index], module.Items[target]) = (module.Items[target], module.Items[index]);
        IsDirty = true;
    }

    private void MovePageToModule(EditModule fromModule, EditPage page, int moduleDelta)
    {
        var fromIndex = Modules.IndexOf(fromModule);
        var toIndex = fromIndex + moduleDelta;
        if (toIndex < 0 || toIndex >= Modules.Count) return;

        fromModule.Items.Remove(page);
        Modules[toIndex].Items.Add(page);
        IsDirty = true;
    }

    private void OnPageDragStart(string moduleText, string url) => _draggingPage = (moduleText, url);

    private void OnModuleDragStart(string moduleText) => _draggingModule = moduleText;

    private void OnDropOnModule(EditModule targetModule)
    {
        if (_draggingPage != null)
        {
            var (fromModuleText, url) = _draggingPage.Value;
            var fromModule = Modules.FirstOrDefault(m => m.Text == fromModuleText);
            var page = fromModule?.Items.FirstOrDefault(p => p.Url == url);
            if (fromModule != null && page != null && fromModule != targetModule)
            {
                fromModule.Items.Remove(page);
                targetModule.Items.Add(page);
                IsDirty = true;
            }
            _draggingPage = null;
        }
        else if (_draggingModule != null)
        {
            var fromIndex = Modules.FindIndex(m => m.Text == _draggingModule);
            var toIndex = Modules.IndexOf(targetModule);
            if (fromIndex >= 0 && toIndex >= 0 && fromIndex != toIndex)
            {
                var module = Modules[fromIndex];
                Modules.RemoveAt(fromIndex);
                Modules.Insert(toIndex, module);
                IsDirty = true;
            }
            _draggingModule = null;
        }
        StateHasChanged();
    }

    private async Task SaveAsync()
    {
        IsBusy = true;
        StateHasChanged();
        try
        {
            var items = new List<MenuOverrideItem>();
            for (var moduleIndex = 0; moduleIndex < Modules.Count; moduleIndex++)
            {
                var module = Modules[moduleIndex];
                items.Add(new MenuOverrideItem { ItemType = "module", ItemKey = module.Text, SortOrder = moduleIndex, Icon = module.Icon });
                for (var pageIndex = 0; pageIndex < module.Items.Count; pageIndex++)
                {
                    var page = module.Items[pageIndex];
                    items.Add(new MenuOverrideItem
                    {
                        ItemType = "page",
                        ItemKey = page.Url,
                        ModuleText = module.Text,
                        // Store a rename only while it actually differs from the code label, so
                        // renaming back to the original leaves no stale row to maintain.
                        TextOverride = page.Text.Equals(page.DefaultText, StringComparison.Ordinal) ? null : page.Text,
                        SortOrder = pageIndex,
                        IsEnabled = page.Enabled,
                    });
                }
            }

            await Client.SaveAsync(items);
            Provider.Invalidate();
            IsDirty = false;
            await Toast.Success("Sắp xếp menu", "Đã lưu. Tải lại trang (F5) để xem thứ tự mới.");
        }
        catch (Exception ex)
        {
            await Toast.Error("Sắp xếp menu", $"Lưu thất bại: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            StateHasChanged();
        }
    }
}
