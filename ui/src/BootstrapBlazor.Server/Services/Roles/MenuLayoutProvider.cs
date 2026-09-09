namespace BootstrapBlazor.Server.Services;

public interface IMenuLayoutProvider
{
    /// <summary>Cached override rows (60s TTL). Fail-open: errors return an empty list, which
    /// makes the merge fall back to PageRegistry's default order — order is UX, not security.</summary>
    Task<List<MenuOverrideItem>> GetOverridesAsync();

    /// <summary>Forces the next GetOverridesAsync call to re-fetch (called after Save on the
    /// admin's own circuit; other users pick up the change within the TTL).</summary>
    void Invalidate();
}

public class MenuLayoutProvider : IMenuLayoutProvider
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

    private readonly IMenuLayoutClientService _client;
    private List<MenuOverrideItem>? _cached;
    private DateTime _cachedAt;

    public MenuLayoutProvider(IMenuLayoutClientService client)
    {
        _client = client;
    }

    public async Task<List<MenuOverrideItem>> GetOverridesAsync()
    {
        if (_cached != null && DateTime.UtcNow - _cachedAt < Ttl)
        {
            return _cached;
        }

        try
        {
            _cached = await _client.GetAsync();
            _cachedAt = DateTime.UtcNow;
            return _cached;
        }
        catch
        {
            return new List<MenuOverrideItem>();
        }
    }

    public void Invalidate()
    {
        _cached = null;
    }
}
