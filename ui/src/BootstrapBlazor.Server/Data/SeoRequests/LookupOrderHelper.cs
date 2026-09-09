namespace BootstrapBlazor.Server.Data.SeoRequests;

public static class LookupOrderHelper
{
    public static IEnumerable<T> OrderByOdx<T>(
        IEnumerable<T> items,
        Func<T, int> getOdx,
        Func<T, string?> getName)
    {
        return items
            .OrderBy(getOdx)
            .ThenBy(getName, StringComparer.OrdinalIgnoreCase);
    }

    public static IEnumerable<T> OrderByNullableOdx<T>(
        IEnumerable<T> items,
        Func<T, int?> getOdx,
        Func<T, string?> getName)
    {
        return items
            .OrderBy(x => getOdx(x) ?? int.MaxValue)
            .ThenBy(getName, StringComparer.OrdinalIgnoreCase);
    }
}
