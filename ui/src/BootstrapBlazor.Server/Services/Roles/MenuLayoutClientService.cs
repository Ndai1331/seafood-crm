using Blazored.LocalStorage;
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

/// <summary>One menu order/module-reassignment override row (mirrors Contract.MenuLayout.MenuOverrideDto).</summary>
public class MenuOverrideItem
{
    public string ItemType { get; set; } = "";
    public string ItemKey { get; set; } = "";
    public string? ModuleText { get; set; }

    // page rows only: sidebar label chosen by an admin, replacing PageRegistry's Text.
    // Null/blank = keep the name from code.
    public string? TextOverride { get; set; }
    public int SortOrder { get; set; }
    public string? Icon { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public interface IMenuLayoutClientService
{
    Task<List<MenuOverrideItem>> GetAsync();
    Task SaveAsync(List<MenuOverrideItem> items);
}

/// <summary>HTTP client for api/menu-layout — GET is a plain authenticated read, POST needs
/// system.menu-arrangement (enforced API-side; UI just calls it).</summary>
public class MenuLayoutClientService : IMenuLayoutClientService
{
    private readonly ILocalStorageService _localStorage;

    public MenuLayoutClientService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<List<MenuOverrideItem>> GetAsync()
    {
        await EnsureRequestAuthAsync();
        var items = await RequestClient.GetAPIAsync<List<MenuOverrideItem>>("menu-layout");
        return items ?? new List<MenuOverrideItem>();
    }

    public async Task SaveAsync(List<MenuOverrideItem> items)
    {
        await EnsureRequestAuthAsync();
        await RequestClient.PostAPIAsync<object>("menu-layout", new { Items = items }, notifyOk: false);
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
