namespace Core.Const
{
    public static class PermissionClaimType
    {
        public const string Permission = "permission";
    }

    public sealed record PermissionDefinition(string Code, string Module, string ModuleText, string Text, string Url);

    public static class Permissions
    {
        public const string ModDashboard = "dashboard";
        public const string ModMaster = "master-data";
        public const string ModInbound = "inbound";
        public const string ModProduction = "production";
        public const string ModInventory = "inventory";
        public const string ModExport = "export";
        public const string ModFinance = "finance";
        public const string ModSystem = "system";

        public const string Dashboard = "dashboard.overview";
        public const string MasterCatalog = "master-data.catalog";
        public const string MasterProducts = "master-data.products";
        public const string MasterCustomers = "master-data.customers";
        public const string MasterPaymentTerms = "master-data.payment-terms";
        public const string MasterDocumentTypes = "master-data.document-types";
        public const string MasterDocumentRules = "master-data.document-rules";
        public const string Inbound = "inbound.purchases";
        public const string Production = "production.lots";
        public const string Inventory = "inventory.stock";
        public const string ExportQuotes = "export.quotes";
        public const string ExportOrders = "export.orders";
        public const string ExportAllocations = "export.allocations";
        public const string FinancePayments = "finance.payments";
        public const string FinanceDeposits = "finance.deposits";
        public const string UserManager = "system.user-manager";
        public const string TeamManager = "system.team";
        public const string SsoSettings = "system.sso-settings";
        public const string PermissionMatrix = "system.permission-matrix";
        public const string MenuArrangement = "system.menu-arrangement";
        public const string SystemAudit = "system.audit-log";

        public static readonly IReadOnlyList<PermissionDefinition> All = new List<PermissionDefinition>
        {
            new(Dashboard, ModDashboard, "Dashboard", "Tổng quan", "dashboard"),
            new(MasterCatalog, ModMaster, "Danh mục", "Danh mục chung", "master-data/catalog"),
            new(MasterProducts, ModMaster, "Danh mục", "Mặt hàng / SKU", "master-data/products"),
            new(MasterCustomers, ModMaster, "Danh mục", "Khách hàng", "master-data/customers"),
            new(MasterPaymentTerms, ModMaster, "Danh mục", "Điều khoản thanh toán", "master-data/payment-terms"),
            new(MasterDocumentTypes, ModMaster, "Danh mục", "Loại chứng từ", "master-data/document-types"),
            new(MasterDocumentRules, ModMaster, "Danh mục", "Quy tắc chứng từ theo thị trường", "master-data/document-rules"),
            new(Inbound, ModInbound, "Nhập hàng", "Nhập nguyên liệu", "inbound"),
            new(Production, ModProduction, "Sản xuất", "Đưa vào sản xuất", "production"),
            new(Inventory, ModInventory, "Kho", "Tồn kho & chứng từ", "inventory"),
            new(ExportQuotes, ModExport, "Xuất khẩu", "Báo giá / Hợp đồng", "export/quotes"),
            new(ExportOrders, ModExport, "Xuất khẩu", "Đơn hàng xuất khẩu", "export/orders"),
            new(ExportAllocations, ModExport, "Xuất khẩu", "Xếp lô & chứng từ", "export/allocations"),
            new(FinancePayments, ModFinance, "Tài chính", "Theo dõi thanh toán", "finance/payments"),
            new(FinanceDeposits, ModFinance, "Tài chính", "Deposit", "finance/deposits"),
            new(UserManager, ModSystem, "Quản trị hệ thống", "Quản lý người dùng", "user-manager"),
            new(TeamManager, ModSystem, "Quản trị hệ thống", "Quản lý team", "team"),
            new(SsoSettings, ModSystem, "Quản trị hệ thống", "Cài đặt đăng nhập", "sso-settings"),
            new(PermissionMatrix, ModSystem, "Quản trị hệ thống", "Phân quyền", "permission-matrix"),
            new(MenuArrangement, ModSystem, "Quản trị hệ thống", "Sắp xếp menu", "menu-arrangement"),
            new(SystemAudit, ModSystem, "Quản trị hệ thống", "Nhật ký hệ thống", "system/audit-log"),
        };
    }
}
