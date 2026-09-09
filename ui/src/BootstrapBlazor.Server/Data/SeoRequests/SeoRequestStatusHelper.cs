namespace BootstrapBlazor.Server.Data.SeoRequests;

/// <summary>
/// Mirrors the API's SeoRequestRoleHelper. Keep both in step — the API is the real gate,
/// this side only decides what to put on screen.
/// </summary>
public static class SeoRequestStatusHelper
{
    private static string NormalizeRole(string? role) =>
        role?.Trim().ToUpperInvariant() ?? string.Empty;

    public static bool HasFullAccess(string? role) =>
        NormalizeRole(role) is "ADMIN" or "SUPER_ADMIN" or "HEAD" or "ASSISTANT" or "IC";

    public static bool IsSeoLeader(string? role) =>
        NormalizeRole(role) is "SEO LEADER" or "SEO MANAGER";

    public static bool IsAdmin(string? role) =>
        NormalizeRole(role) is "ADMIN" or "SUPER_ADMIN";

    /// <summary>
    /// Roles that file tickets of their own, as opposed to Head/Assistant/IC who only approve
    /// other people's. Admin counts: they own tickets and run the tool day to day.
    /// </summary>
    public static bool OwnsRequests(string? role) =>
        IsSeoOnly(role) || IsSeoLeader(role) || IsAdmin(role);

    public static bool IsSeoOnly(string? role)
    {
        var normalized = NormalizeRole(role);
        return !HasFullAccess(normalized)
            && !IsSeoLeader(normalized)
            && normalized.Contains("SEO", StringComparison.Ordinal);
    }

    /// <summary>QC audits SEO spending: sees every ticket, changes nothing.</summary>
    public static bool IsReadOnly(string? role) =>
        NormalizeRole(role) == "QC";

    public static bool IsKnownRole(string? role) =>
        HasFullAccess(role) || IsSeoLeader(role) || IsSeoOnly(role) || IsReadOnly(role);

    public static bool CanViewAll(string? role) =>
        HasFullAccess(role) || IsReadOnly(role);

    /// <summary>May change anything at all. Read-only and unknown roles never can.</summary>
    public static bool CanWrite(string? role) =>
        IsKnownRole(role) && !IsReadOnly(role);

    public static bool CanCreate(string? role) => CanWrite(role);

    /// <summary>Only these roles may edit a ticket that already cleared all four approval steps.</summary>
    public static bool CanEditAfterFullApproval(string? role) =>
        NormalizeRole(role) is "ADMIN" or "SUPER_ADMIN" or "HEAD";

    public static bool CanEdit(SeoRequestDto item, int currentUserId, string? role)
    {
        if (!CanWrite(role))
            return false;

        // Approved amount must not drift from the paid amount — ADMIN/HEAD correct it if needed.
        if (IsFullyApproved(item) && !CanEditAfterFullApproval(role))
            return false;

        if (HasFullAccess(role) || IsSeoLeader(role))
            return true;

        if (IsSeoOnly(role))
            return item.PicId == currentUserId;

        return false;
    }

    public static bool CanDelete(SeoRequestDto item, int currentUserId, string? role)
    {
        if (!CanWrite(role) || IsFullyApproved(item))
            return false;

        if (HasFullAccess(role))
            return true;

        if (IsSeoOnly(role))
        {
            if (item.PicId != currentUserId)
                return false;

            // Tiền đã đi thì phiếu là chứng từ — PIC không xoá, kể cả khi Lead chưa xử lý.
            // Giống hệt gate của API (SeoRequestRoleHelper.CanSeoDelete); lệch là nút hiện
            // ra rồi lệnh lưu bị từ chối.
            if (SeoPaymentStatuses.IsPaidStatus(item.PaymentStatus))
                return false;

            return item.PaymentStatus == SeoPaymentStatuses.Draft
                   || string.IsNullOrEmpty(item.LeadStatus)
                   || item.LeadStatus == SeoRequestApprovalStatuses.ChoCheck;
        }

        return IsSeoLeader(role);
    }

    /// <summary>Lead + IC + Assistant + Head all approved — mirror API delete gate.</summary>
    public static bool IsFullyApproved(SeoRequestDto item) =>
        IsLeadApproved(item.LeadStatus)
        && string.Equals(NormalizeStatus(item.IcStatus), SeoRequestApprovalStatuses.DaCheck, StringComparison.Ordinal)
        && IsAssistantApproved(item.FinalCheckStatus)
        && IsHeadApproved(item.HeadStatus);

    private static bool IsLeadApproved(string? status) =>
        NormalizeStatus(status) is "DUYET" or "DA_DUYET";

    private static bool IsAssistantApproved(string? status) =>
        NormalizeStatus(status) is "DUYET" or "DA_DUYET" or "OK";

    /// <summary>
    /// Head's signature is the gate for the whole execution half of the lifecycle, so this
    /// answer has to be the same everywhere it is asked — mirrors the API's own check.
    /// </summary>
    public static bool IsHeadApproved(string? status) =>
        NormalizeStatus(status) is "DUYET" or "DA_DUYET" or "OK";

    /// <summary>Payment/settlement statuses (Đã thanh toán, Đã nghiệm thu, …) are a finance decision.</summary>
    public static bool CanUpdatePaymentStatus(string? role) =>
        NormalizeRole(role) is "ADMIN" or "SUPER_ADMIN" or "HEAD" or "ASSISTANT";

    /// <summary>
    /// Chờ/Đang/Đã triển khai after payment: staff roles, or the PIC who owns the ticket.
    /// </summary>
    public static bool CanUpdateImplementStatus(SeoRequestDto item, int currentUserId, string? role)
    {
        if (!CanWrite(role))
            return false;

        if (HasFullAccess(role) || IsSeoLeader(role))
            return true;

        return item.PicId == currentUserId;
    }

    public static bool CanUpdatePostPurchaseStatus(SeoRequestDto item, int currentUserId, string? role) =>
        CanUpdateImplementStatus(item, currentUserId, role);

    /// <summary>
    /// Whether the final-status dropdown should appear at all, and with which options.
    /// Finance roles get the full list; a PIC only gets the implement steps, and only
    /// once the ticket is paid.
    /// </summary>
    public static bool CanChangeAnyPaymentStatus(SeoRequestDto item, int currentUserId, string? role) =>
        CanUpdatePaymentStatus(role)
        || (CanUpdateImplementStatus(item, currentUserId, role) && IsInImplementFlow(item.PaymentStatus));

    /// <summary>Paid, or already partway through Chờ → Đang → Đã triển khai.</summary>
    private static bool IsInImplementFlow(string? paymentStatus) =>
        SeoPaymentStatuses.IsPaidStatus(paymentStatus) || SeoPaymentStatuses.IsImplementStatus(paymentStatus);

    public static string GetRequestTypeLabel(string? requestType) => requestType switch
    {
        SeoRequestTypes.SeoResource => "Đề xuất SEO Resource",
        SeoRequestTypes.DomainPurchase => "Mua domain",
        _ => requestType ?? "-"
    };

    public static string GetPaymentStatusLabel(string? status) => status switch
    {
        SeoPaymentStatuses.Draft => "Nháp",
        SeoPaymentStatuses.ChoDuyetTrienKhai => "Chờ duyệt triển khai",
        SeoPaymentStatuses.DangTrienKhai => "Đang triển khai",
        SeoPaymentStatuses.ChoTrienKhai => "Chờ triển khai",
        SeoPaymentStatuses.DaTrienKhai => "Đã triển khai",
        SeoPaymentStatuses.DaNghiemThu => "Đã nghiệm thu",
        SeoPaymentStatuses.ChoDuyetThanhToan => "Chờ duyệt thanh toán",
        SeoPaymentStatuses.DaThanhToan => "Đã thanh toán",
        SeoPaymentStatuses.KhongTrienKhai => "Không triển khai",
        SeoPaymentStatuses.ChoDealGia => "Chờ deal giá",
        SeoPaymentStatuses.ChoCheckLaiSau => "Chờ check lại sau",
        SeoPaymentStatuses.HoanThanh => "Đã thanh toán",
        SeoPaymentStatuses.TuChoi => "Từ chối",
        _ => status ?? "-"
    };

    public static (string BgColor, string TextColor) GetPaymentStatusPillStyle(string? status)
    {
        var match = SeoPaymentStatuses.FinalStatusOptions
            .FirstOrDefault(x => x.Code == status);

        if (match != default)
            return ("transparent", match.TextColor);

        return status switch
        {
            SeoPaymentStatuses.Draft => ("transparent", SeoColoredOptionHelper.ColorNeutral),
            SeoPaymentStatuses.HoanThanh => ("transparent", SeoColoredOptionHelper.ColorSuccess),
            SeoPaymentStatuses.TuChoi => ("transparent", SeoColoredOptionHelper.ColorDanger),
            _ => ("transparent", SeoColoredOptionHelper.ColorNeutral)
        };
    }

    public static string GetLeadStatusLabel(string? status) => NormalizeStatus(status) switch
    {
        SeoRequestApprovalStatuses.ChoCheck => "Chờ check",
        SeoRequestApprovalStatuses.Duyet or "DA_DUYET" => "Duyệt",
        SeoRequestApprovalStatuses.KhongDuyet or "TU_CHOI" => "Không duyệt",
        "YEU_CAU_SUA" => "Yêu cầu chỉnh sửa",
        _ => status ?? "Chờ check"
    };

    public static string GetIcStatusLabel(string? status) => NormalizeStatus(status) switch
    {
        SeoRequestApprovalStatuses.ChoCheck => "Chờ check",
        SeoRequestApprovalStatuses.DaCheck => "Đã check",
        SeoRequestApprovalStatuses.KhongXuLy => "Không xử lý",
        SeoRequestApprovalStatuses.ChoPicFix => "Chờ PIC fix",
        SeoRequestApprovalStatuses.XuLySau => "Xử lý sau",
        "TU_CHOI" => "Không duyệt",
        _ => status ?? "Chờ check"
    };

    public static string GetHeadStatusLabel(string? status) => NormalizeStatus(status) switch
    {
        "CHO_DUYET" => "Chờ duyệt",
        SeoRequestApprovalStatuses.Duyet or "DA_DUYET" => "Duyệt",
        SeoRequestApprovalStatuses.KhongDuyet or "TU_CHOI" => "Không duyệt",
        _ => status ?? "Chờ duyệt"
    };

    public static string GetAssistantStatusLabel(string? status) => NormalizeStatus(status) switch
    {
        "CHO_DUYET" => "Chờ duyệt",
        SeoRequestApprovalStatuses.Duyet or "DA_DUYET" => "Duyệt",
        SeoRequestApprovalStatuses.KhongDuyet or "TU_CHOI" => "Không duyệt",
        "YEU_CAU_SUA" => "Yêu cầu chỉnh sửa",
        _ => status ?? "Chờ duyệt"
    };

    public static string GetStepStatusLabel(string step, string? status) => step.ToUpperInvariant() switch
    {
        SeoRequestApprovalSteps.Lead => GetLeadStatusLabel(status),
        SeoRequestApprovalSteps.Ic => GetIcStatusLabel(status),
        SeoRequestApprovalSteps.Assistant => GetAssistantStatusLabel(status),
        SeoRequestApprovalSteps.Head => GetHeadStatusLabel(status),
        _ => status ?? "-"
    };

    private static string? NormalizeStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();

    public static bool CanApproveStep(string? role, string step)
    {
        if (!CanWrite(role))
            return false;

        // Mirrors the API's own gate. Each step belongs to one role and only admin may sign
        // someone else's; keep the two in step or the rail shows controls the save will reject.
        if (IsAdmin(role))
            return true;

        var normalized = NormalizeRole(role);
        return step switch
        {
            SeoRequestApprovalSteps.Lead => normalized is "SEO LEADER" or "SEO MANAGER",
            SeoRequestApprovalSteps.Ic => normalized == "IC",
            SeoRequestApprovalSteps.Assistant => normalized == "ASSISTANT",
            SeoRequestApprovalSteps.Head => normalized == "HEAD",
            _ => false
        };
    }

    /// <summary>
    /// Lower steps locked when next step left pending. ADMIN overrides.
    /// </summary>
    public static bool CanEditApprovalStep(
        string? role,
        string step,
        string? icStatus,
        string? assistantStatus,
        string? headStatus)
    {
        if (NormalizeRole(role) is "ADMIN" or "SUPER_ADMIN")
            return true;

        return step switch
        {
            SeoRequestApprovalSteps.Lead => IsIcPending(icStatus),
            SeoRequestApprovalSteps.Ic => IsAssistantPending(assistantStatus),
            SeoRequestApprovalSteps.Assistant => IsHeadPending(headStatus),
            SeoRequestApprovalSteps.Head => true,
            _ => false
        };
    }

    public static bool IsApprovalStepLockedByNext(
        string? role,
        string step,
        string? icStatus,
        string? assistantStatus,
        string? headStatus) =>
        CanApproveStep(role, step)
        && !CanEditApprovalStep(role, step, icStatus, assistantStatus, headStatus);

    private static bool IsIcPending(string? status) =>
        string.Equals(NormalizeStatus(status), SeoRequestApprovalStatuses.ChoCheck, StringComparison.Ordinal);

    private static bool IsAssistantPending(string? status)
    {
        var n = NormalizeStatus(status);
        return n is null or SeoRequestApprovalStatuses.ChoCheck or "CHO_DUYET";
    }

    private static bool IsHeadPending(string? status)
    {
        var n = NormalizeStatus(status);
        return n is null or "CHO_DUYET";
    }

    /// <summary>Seeing the approval trail — QC audits it, but every control stays disabled.</summary>
    public static bool CanViewApproval(SeoRequestDto item, int currentUserId, string? role) =>
        CanViewAll(role) || IsSeoLeader(role) || item.PicId == currentUserId;

    public static string GetLogActionLabel(string action) => action switch
    {
        "CREATE" => "Tạo mới request",
        "PAYMENT" => "IC ghi ID KT — đã chuyển tiền",
        "UPDATE" => "Cập nhật request",
        "LEAD_STATUS" => "Lead cập nhật trạng thái",
        "IC_STATUS" => "IC cập nhật trạng thái",
        "ASSISTANT_STATUS" => "Assistant cập nhật trạng thái",
        "HEAD_STATUS" => "Head cập nhật trạng thái",
        "LEAD_APPROVE" => "Lead duyệt",
        "LEAD_REJECT" => "Lead từ chối",
        "LEAD_REQUEST_EDIT" => "Lead yêu cầu chỉnh sửa",
        "IC_APPROVE" => "IC đã check",
        "IC_REJECT" => "IC từ chối",
        "ASSISTANT_APPROVE" => "Assistant duyệt",
        "ASSISTANT_REJECT" => "Assistant từ chối",
        "ASSISTANT_REQUEST_EDIT" => "Assistant yêu cầu chỉnh sửa",
        "HEAD_APPROVE" => "Head duyệt",
        "HEAD_REJECT" => "Head từ chối",
        _ => action ?? "-"
    };

    public static string GetStatusBadgeClass(string? status) => NormalizeStatus(status) switch
    {
        "DRAFT" => "bg-secondary",
        "CHO_DUYET_TRIEN_KHAI" or SeoRequestApprovalStatuses.ChoCheck or "CHO_DUYET"
            or SeoRequestApprovalStatuses.ChoPicFix or SeoRequestApprovalStatuses.XuLySau => "bg-warning text-dark",
        SeoRequestApprovalStatuses.Duyet or "DA_DUYET" or SeoRequestApprovalStatuses.DaCheck or "HOAN_THANH" => "bg-success",
        SeoRequestApprovalStatuses.KhongDuyet or "TU_CHOI" or SeoRequestApprovalStatuses.KhongXuLy => "bg-danger",
        _ => "bg-info"
    };

    private static readonly System.Globalization.CultureInfo EnUs =
        System.Globalization.CultureInfo.GetCultureInfo("en-US");

    public static string FormatVnd(decimal? value)
    {
        if (!value.HasValue)
            return "0";

        return value.Value.ToString("#,##0.##", EnUs);
    }

    public static string FormatUsdt(decimal? value)
    {
        if (!value.HasValue)
            return "0";

        return value.Value.ToString("#,##0.##", EnUs);
    }

    public static string TruncateNote(string? value, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : $"{trimmed[..maxLength]}…";
    }

    public static string FormatCompactMoney(decimal value)
    {
        if (value >= 1_000_000_000)
            return (value / 1_000_000_000m).ToString("0.#", EnUs) + "B";
        if (value >= 1_000_000)
            return (value / 1_000_000m).ToString("0.#", EnUs) + "M";
        if (value >= 1_000)
            return (value / 1_000m).ToString("0.#", EnUs) + "K";
        return value.ToString("0", EnUs);
    }
}
