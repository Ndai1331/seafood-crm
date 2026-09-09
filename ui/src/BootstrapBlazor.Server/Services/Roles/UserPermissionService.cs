using Blazored.LocalStorage;
using BootstrapBlazor.Server.Http;

namespace BootstrapBlazor.Server.Services;

public interface IUserPermissionService
{
    /// <summary>Permission codes of the current user (cached per access token).</summary>
    Task<HashSet<string>> GetPermissionsAsync();
    Task<(HashSet<string> Permissions, string? DefaultPageUrl)> GetPermissionsWithDefaultAsync();

    /// <summary>True when the current user holds the permission code.</summary>
    Task<bool> HasAsync(string permissionCode);
}

/// <summary>
/// Scoped cache of the current user's permission codes, fetched from
/// GET api/role/my-permissions. Re-fetches automatically when the access token
/// changes (login/logout), so permission edits take effect on next login or
/// within the API-side cache TTL. Fails CLOSED: on fetch errors returns an
/// empty set (menu hides, guard denies) without caching the failure.
/// </summary>
public class UserPermissionService : IUserPermissionService
{
    private readonly ILocalStorageService _localStorage;
    private HashSet<string>? _permissions;
    private string? _defaultPageUrl;
    private string? _loadedForToken;

    public UserPermissionService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    private sealed class MyPermissionsResultDto
    {
        public List<string> Permissions { get; set; } = new();
        public string? DefaultPageUrl { get; set; }
    }

    public async Task<HashSet<string>> GetPermissionsAsync()
    {
        var result = await GetPermissionsWithDefaultAsync();
        return result.Permissions;
    }

    public async Task<(HashSet<string> Permissions, string? DefaultPageUrl)> GetPermissionsWithDefaultAsync()
    {
        string? token = null;
        try
        {
            token = await _localStorage.GetItemAsync<string>("my-access-token");
        }
        catch
        {
            // Prerendering: JS interop unavailable — treat as not logged in yet.
        }

        if (string.IsNullOrWhiteSpace(token))
        {
#if DEBUG
            // Local dev without login: legacy behavior granted full ADMIN — grant the whole catalog.
            return (PageRegistry.AllEntries().Select(e => e.Permission)
                .ToHashSet(StringComparer.OrdinalIgnoreCase), null);
#else
            return (new HashSet<string>(StringComparer.OrdinalIgnoreCase), null);
#endif
        }

        if (_permissions != null && _loadedForToken == token)
        {
            return (_permissions, _defaultPageUrl);
        }

        try
        {
            RequestClient.InjectServices(_localStorage);
            RequestClient.AttachToken(token);
            var result = await RequestClient.GetAPIAsync<MyPermissionsResultDto>("role/my-permissions");
            _permissions = new HashSet<string>(result?.Permissions ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            _defaultPageUrl = result?.DefaultPageUrl;
            _loadedForToken = token;
            return (_permissions, _defaultPageUrl);
        }
        catch
        {
            // Fail closed, don't cache — next call retries.
            return (new HashSet<string>(StringComparer.OrdinalIgnoreCase), null);
        }
    }

    public async Task<bool> HasAsync(string permissionCode)
    {
        var permissions = await GetPermissionsAsync();
        return permissions.Contains(permissionCode);
    }
}
