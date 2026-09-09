namespace BootstrapBlazor.Server.Data.SeoRequests;

public static class SeoPaymentStatuses
{
    public const string Draft = "DRAFT";
    public const string ChoDuyetTrienKhai = "CHO_DUYET_TRIEN_KHAI";
    public const string DangTrienKhai = "DANG_TRIEN_KHAI";
    public const string ChoDuyetThanhToan = "CHO_DUYET_THANH_TOAN";
    public const string DaThanhToan = "DA_THANH_TOAN";
    public const string KhongTrienKhai = "KHONG_TRIEN_KHAI";
    public const string ChoDealGia = "CHO_DEAL_GIA";
    public const string ChoCheckLaiSau = "CHO_CHECK_LAI_SAU";
    public const string ChoTrienKhai = "CHO_TRIEN_KHAI";
    public const string DaTrienKhai = "DA_TRIEN_KHAI";
    public const string DaNghiemThu = "DA_NGHIEM_THU";

    // Legacy values kept for existing records / approval workflow
    public const string HoanThanh = "HOAN_THANH";
    public const string TuChoi = "TU_CHOI";

    /// <summary>
    /// Listed in business order so the picker reads like the flow itself: approve → implement
    /// → accept → pay, with the off-ramps last. It used to lead with "Đã thanh toán", which
    /// implied money came before the work.
    /// </summary>
    public static readonly IReadOnlyList<(string Code, string Label, string TextColor)> FinalStatusOptions =
    [
        (ChoDuyetTrienKhai, "Chờ duyệt triển khai", SeoColoredOptionHelper.ColorWarning),
        (ChoTrienKhai, "Chờ triển khai", SeoColoredOptionHelper.ColorWarning),
        (DangTrienKhai, "Đang triển khai", SeoColoredOptionHelper.ColorPrimary),
        (DaTrienKhai, "Đã triển khai", SeoColoredOptionHelper.ColorSuccess),
        (DaNghiemThu, "Đã nghiệm thu", SeoColoredOptionHelper.ColorInfo),
        (ChoDuyetThanhToan, "Chờ duyệt thanh toán", SeoColoredOptionHelper.ColorInfo),
        (DaThanhToan, "Đã thanh toán", SeoColoredOptionHelper.ColorSuccess),
        (KhongTrienKhai, "Không triển khai", SeoColoredOptionHelper.ColorDanger),
        (ChoDealGia, "Chờ deal giá", SeoColoredOptionHelper.ColorWarning),
        (ChoCheckLaiSau, "Chờ check lại sau", SeoColoredOptionHelper.ColorNeutral)
    ];

    public static readonly HashSet<string> AllowedValues = new(StringComparer.Ordinal)
    {
        Draft,
        ChoDuyetTrienKhai,
        DangTrienKhai,
        ChoDuyetThanhToan,
        DaThanhToan,
        KhongTrienKhai,
        ChoDealGia,
        ChoCheckLaiSau,
        ChoTrienKhai,
        DaTrienKhai,
        DaNghiemThu,
        HoanThanh,
        TuChoi
    };

    public static readonly HashSet<string> ImplementStatuses = new(StringComparer.Ordinal)
    {
        ChoTrienKhai,
        DangTrienKhai,
        DaTrienKhai
    };

    public static bool IsImplementStatus(string? status) =>
        !string.IsNullOrWhiteSpace(status) && ImplementStatuses.Contains(status);

    public static bool IsPaidStatus(string? status) =>
        status is DaThanhToan or HoanThanh;

    public static List<ColoredSelectOption> BuildFinalStatusOptions() =>
        ToOptions(FinalStatusOptions);

    /// <summary>
    /// Business lifecycle order: approve → implement → accept → pay.
    /// Mirrors SeoPaymentStatusConstants.Lifecycle on the API side, which stays authoritative.
    /// </summary>
    private static readonly string[] Lifecycle =
    [
        ChoDuyetTrienKhai,
        ChoTrienKhai,
        DangTrienKhai,
        DaTrienKhai,
        DaNghiemThu,
        ChoDuyetThanhToan,
        DaThanhToan
    ];

    /// <summary>
    /// States that take a ticket off the timeline: cancelled, refused, price still being
    /// negotiated, parked. The rail stops rather than pretending the ticket is still moving.
    /// </summary>
    public static bool IsOffChain(string? status) =>
        status is KhongTrienKhai or ChoDealGia or ChoCheckLaiSau or TuChoi or Draft;

    public static int LifecycleRank(string? status) =>
        string.IsNullOrWhiteSpace(status) ? -1 : Array.IndexOf(Lifecycle, status);

    /// <summary>
    /// Only the moves the server will accept for this ticket: nothing past the approval stage
    /// before Head signed off, and no rewinding for roles that may not correct a ticket.
    /// Offering the rest just invites a click that comes back as an error toast.
    /// </summary>
    public static List<ColoredSelectOption> BuildFinalStatusOptionsFor(
        string? currentStatus,
        string? headStatus,
        bool canRewind = true)
    {
        var headApproved = SeoRequestStatusHelper.IsHeadApproved(headStatus);
        var fromRank = Math.Max(LifecycleRank(currentStatus), 0);

        var allowed = FinalStatusOptions.Where(x =>
        {
            // Always keep the ticket's own value so the picker can render it.
            if (x.Code == currentStatus)
                return true;

            var rank = LifecycleRank(x.Code);

            // Off-chain (huỷ, chờ deal giá, chờ check lại sau) stays reachable at any time.
            if (rank < 0)
                return true;

            if (rank >= 1 && !headApproved)
                return false;

            return rank >= fromRank || canRewind;
        }).ToList();

        return ToOptions(allowed);
    }

    /// <summary>
    /// Chờ/Đang/Đã triển khai only — what a PIC may move their own approved ticket through.
    /// Everything else on the final-status list is a finance decision.
    /// </summary>
    public static List<ColoredSelectOption> BuildImplementStatusOptions() =>
        ToOptions(FinalStatusOptions.Where(x => ImplementStatuses.Contains(x.Code)).ToList());

    private static List<ColoredSelectOption> ToOptions(
        IReadOnlyList<(string Code, string Label, string TextColor)> source) =>
        source.Select(x => new ColoredSelectOption
        {
            Value = x.Code,
            Text = x.Label,
            BgColor = "transparent",
            TextColor = x.TextColor
        }).ToList();
}
