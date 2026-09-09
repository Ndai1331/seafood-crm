using BootstrapBlazor.Components;

namespace BootstrapBlazor.Server.Data.SeoRequests;

public static class SeoRequestApprovalOptions
{
    public static List<SelectedItem<string>> Lead { get; } =
    [
        new(SeoRequestApprovalStatuses.ChoCheck, "Chờ check"),
        new(SeoRequestApprovalStatuses.Duyet, "Duyệt"),
        new(SeoRequestApprovalStatuses.KhongDuyet, "Không duyệt")
    ];

    public static List<SelectedItem<string>> Ic { get; } =
    [
        new(SeoRequestApprovalStatuses.ChoCheck, "Chờ check"),
        new(SeoRequestApprovalStatuses.DaCheck, "Đã check"),
        new(SeoRequestApprovalStatuses.KhongXuLy, "Không xử lý"),
        new(SeoRequestApprovalStatuses.ChoPicFix, "Chờ PIC fix"),
        new(SeoRequestApprovalStatuses.XuLySau, "Xử lý sau")
    ];

    public static List<SelectedItem<string>> Assistant { get; } =
    [
        new(SeoRequestApprovalStatuses.Duyet, "Duyệt"),
        new(SeoRequestApprovalStatuses.KhongDuyet, "Không duyệt")
    ];

    public static List<SelectedItem<string>> Head { get; } =
    [
        new(SeoRequestApprovalStatuses.Duyet, "Duyệt"),
        new(SeoRequestApprovalStatuses.KhongDuyet, "Không duyệt")
    ];

    /// <summary>List filter: detail options + pending "Chờ duyệt".</summary>
    public static List<SelectedItem<string>> AssistantFilter { get; } =
    [
        new("CHO_DUYET", "Chờ duyệt"),
        new(SeoRequestApprovalStatuses.Duyet, "Duyệt"),
        new(SeoRequestApprovalStatuses.KhongDuyet, "Không duyệt")
    ];

    /// <summary>List filter: detail options + pending "Chờ duyệt".</summary>
    public static List<SelectedItem<string>> HeadFilter { get; } =
    [
        new("CHO_DUYET", "Chờ duyệt"),
        new(SeoRequestApprovalStatuses.Duyet, "Duyệt"),
        new(SeoRequestApprovalStatuses.KhongDuyet, "Không duyệt")
    ];
}

public static class SeoRequestApprovalStatuses
{
    public const string ChoCheck = "CHO_CHECK";
    public const string Duyet = "DUYET";
    public const string KhongDuyet = "KHONG_DUYET";
    public const string DaCheck = "DA_CHECK";
    public const string KhongXuLy = "KHONG_XU_LY";
    public const string ChoPicFix = "CHO_PIC_FIX";
    public const string XuLySau = "XU_LY_SAU";
}
