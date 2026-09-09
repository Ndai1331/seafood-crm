namespace BootstrapBlazor.Server.Services;

public sealed class PageEntry
{
    public required string Url { get; init; }
    public required string Text { get; init; }
    public string Icon { get; init; } = "";
    public required string Permission { get; init; }
    public bool Hidden { get; init; }
    public List<PageEntry>? Children { get; init; }
}

public sealed class PageModule
{
    public required string Text { get; init; }
    public string Icon { get; init; } = "";
    public required List<PageEntry> Items { get; init; }
}

public static class PageRegistry
{
    public static readonly HashSet<string> PublicUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        "", "login", "unauthorized", "privacy", "terms", "my-profile",
    };

    public const string PreferredDefaultUrl = "dashboard";

    public static readonly IReadOnlyList<PageModule> Modules = new List<PageModule>
    {
        new()
        {
            Text = "Dashboard", Icon = "fa-solid fa-gauge-high",
            Items = new List<PageEntry>
            {
                new() { Text = "Tổng quan", Icon = "fa-fw fa-solid fa-chart-pie", Url = "dashboard", Permission = "dashboard.overview" },
            }
        },
        new()
        {
            Text = "Danh mục", Icon = "fa-solid fa-layer-group",
            Items = new List<PageEntry>
            {
                new() { Text = "Danh mục chung", Icon = "fa-fw fa-solid fa-list", Url = "master-data/catalog", Permission = "master-data.catalog" },
                new() { Text = "Mặt hàng / SKU", Icon = "fa-fw fa-solid fa-fish", Url = "master-data/products", Permission = "master-data.products" },
                new() { Text = "Khách hàng", Icon = "fa-fw fa-solid fa-users", Url = "master-data/customers", Permission = "master-data.customers" },
                new() { Text = "Điều khoản TT", Icon = "fa-fw fa-solid fa-file-invoice-dollar", Url = "master-data/payment-terms", Permission = "master-data.payment-terms" },
            }
        },
        new()
        {
            Text = "Nhập hàng", Icon = "fa-solid fa-truck",
            Items = new List<PageEntry>
            {
                new() { Text = "Nhập nguyên liệu", Icon = "fa-fw fa-solid fa-boxes-stacked", Url = "inbound", Permission = "inbound.purchases" },
            }
        },
        new()
        {
            Text = "Sản xuất", Icon = "fa-solid fa-industry",
            Items = new List<PageEntry>
            {
                new() { Text = "Đưa vào sản xuất", Icon = "fa-fw fa-solid fa-gears", Url = "production", Permission = "production.lots" },
            }
        },
        new()
        {
            Text = "Kho", Icon = "fa-solid fa-warehouse",
            Items = new List<PageEntry>
            {
                new() { Text = "Tồn kho & chứng từ", Icon = "fa-fw fa-solid fa-boxes-packing", Url = "inventory", Permission = "inventory.stock" },
            }
        },
        new()
        {
            Text = "Xuất khẩu", Icon = "fa-solid fa-ship",
            Items = new List<PageEntry>
            {
                new() { Text = "Báo giá / Hợp đồng", Icon = "fa-fw fa-solid fa-file-signature", Url = "export/quotes", Permission = "export.quotes" },
                new() { Text = "Đơn hàng xuất khẩu", Icon = "fa-fw fa-solid fa-file-export", Url = "export/orders", Permission = "export.orders" },
            }
        },
        new()
        {
            Text = "Tài chính", Icon = "fa-solid fa-coins",
            Items = new List<PageEntry>
            {
                new() { Text = "Theo dõi thanh toán", Icon = "fa-fw fa-solid fa-money-check-dollar", Url = "finance/payments", Permission = "finance.payments" },
                new() { Text = "Deposit", Icon = "fa-fw fa-solid fa-piggy-bank", Url = "finance/deposits", Permission = "finance.deposits" },
            }
        },
        new()
        {
            Text = "Quản trị hệ thống", Icon = "fa-solid fa-gear",
            Items = new List<PageEntry>
            {
                new() { Text = "Quản lý người dùng", Icon = "fa-fw fa-solid fa-user-gear", Url = "user-manager", Permission = "system.user-manager" },
                new() { Text = "Quản lý team", Icon = "fa-fw fa-solid fa-people-group", Url = "team", Permission = "system.team" },
                new() { Text = "Cài đặt đăng nhập", Icon = "fa-fw fa-solid fa-shield-halved", Url = "sso-settings", Permission = "system.sso-settings" },
                new() { Text = "Phân quyền", Icon = "fa-fw fa-solid fa-lock", Url = "permission-matrix", Permission = "system.permission-matrix" },
                new() { Text = "Sắp xếp menu", Icon = "fa-fw fa-solid fa-bars", Url = "menu-arrangement", Permission = "system.menu-arrangement" },
            }
        },
    };

    public static IEnumerable<PageEntry> AllEntries()
    {
        foreach (var module in Modules)
        {
            foreach (var item in module.Items)
            {
                yield return item;
                if (item.Children == null) continue;
                foreach (var child in item.Children)
                {
                    yield return child;
                }
            }
        }
    }
}
