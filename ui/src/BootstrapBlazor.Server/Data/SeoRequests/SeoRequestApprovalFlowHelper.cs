namespace BootstrapBlazor.Server.Data.SeoRequests;

/// <summary>Where one approval step stands, for the progress rail.</summary>
public enum SeoApprovalStepState
{
    /// <summary>Approved / checked — the ticket moved past this step.</summary>
    Done,

    /// <summary>Rejected or sent back; the ticket stops here until someone acts.</summary>
    Rejected,

    /// <summary>The step the ticket is sitting on right now.</summary>
    Current,

    /// <summary>Not reached yet — the ticket is still upstream.</summary>
    Waiting
}

/// <summary>What the approval card hands back to the page that owns the API call.</summary>
public sealed record SeoApprovalSubmission(
    string Step,
    string? Status,
    string Comment,
    string? TicketNumber,
    DateTime? TicketDate);

public sealed record SeoApprovalStepView(
    string Code,
    int Ordinal,
    string Title,
    string StatusLabel,
    SeoApprovalStepState State,
    string? ActorName,
    DateTime? ActedAt,
    string? Note,
    /// <summary>False for the execution half, where the "step" is a payment_status value.</summary>
    bool IsApprovalStep = true,
    /// <summary>Rail label — ten nodes across one modal leaves no room for full names.</summary>
    string? ShortTitle = null);

/// <summary>
/// Turns the four independent status columns into one ordered chain, so the detail view can show
/// where a ticket actually is instead of four look-alike panels the reader has to diff by eye.
/// </summary>
public static class SeoRequestApprovalFlowHelper
{
    public static List<SeoApprovalStepView> Build(SeoRequestDetailDto d)
    {
        var leadDone = IsAny(d.LeadStatus, "DUYET", "DA_DUYET");
        var icDone = IsAny(d.IcStatus, "DA_CHECK");
        var assistDone = IsAny(d.AssistantStatus, "DUYET", "DA_DUYET", "OK");
        var headDone = IsAny(d.HeadStatus, "DUYET", "DA_DUYET", "OK");

        var leadRejected = IsAny(d.LeadStatus, "KHONG_DUYET", "TU_CHOI");
        // IC alone can park a ticket (chờ PIC fix / xử lý sau); those stop the chain just like a
        // rejection, so the rail must not paint them as "still waiting for IC".
        var icRejected = IsAny(d.IcStatus, "KHONG_XU_LY", "CHO_PIC_FIX", "XU_LY_SAU", "KHONG_DUYET", "TU_CHOI");
        var assistRejected = IsAny(d.AssistantStatus, "KHONG_DUYET", "TU_CHOI");
        var headRejected = IsAny(d.HeadStatus, "KHONG_DUYET", "TU_CHOI");

        // Only one step can be "current": the first unresolved one whose predecessor is done.
        var currentAssigned = false;

        var steps = new List<SeoApprovalStepView>
        {
            Step(SeoRequestApprovalSteps.Lead, 1, "Lead duyệt",
                SeoRequestStatusHelper.GetLeadStatusLabel(d.LeadStatus),
                Resolve(leadDone, leadRejected, reached: true, ref currentAssigned),
                d.LeadReviewerName, d.LeadReviewedAt, d.LeadNote),

            Step(SeoRequestApprovalSteps.Ic, 2, "IC check",
                SeoRequestStatusHelper.GetIcStatusLabel(d.IcStatus),
                Resolve(icDone, icRejected, reached: leadDone, ref currentAssigned),
                d.IcCheckerName, d.IcCheckedAt, d.IcNote),

            Step(SeoRequestApprovalSteps.Assistant, 3, "Assistant duyệt",
                SeoRequestStatusHelper.GetAssistantStatusLabel(d.AssistantStatus),
                Resolve(assistDone, assistRejected, reached: icDone, ref currentAssigned),
                null, null, d.AssistantNote),

            Step(SeoRequestApprovalSteps.Head, 4, "Head duyệt",
                SeoRequestStatusHelper.GetHeadStatusLabel(d.HeadStatus),
                Resolve(headDone, headRejected, reached: assistDone, ref currentAssigned),
                d.HeadApproverName, d.HeadApprovedAt, d.HeadNote)
        };

        // Execution half — one ticket, one timeline. These are payment_status values, not
        // approval columns, but to the reader they are simply the next stages of the same
        // journey: the partner works, then the partner is paid.
        steps.AddRange(BuildExecutionStages(d, headDone, ref currentAssigned));
        return steps;
    }

    private static readonly (string Code, string Title)[] ExecutionStages =
    [
        (SeoPaymentStatuses.ChoTrienKhai, "Chờ triển khai"),
        (SeoPaymentStatuses.DangTrienKhai, "Đang triển khai"),
        (SeoPaymentStatuses.DaTrienKhai, "Đã triển khai"),
        (SeoPaymentStatuses.DaNghiemThu, "Nghiệm thu"),
        (SeoPaymentStatuses.ChoDuyetThanhToan, "Chờ duyệt TT"),
        (SeoPaymentStatuses.DaThanhToan, "Đã thanh toán")
    ];

    private static List<SeoApprovalStepView> BuildExecutionStages(
        SeoRequestDetailDto d, bool headDone, ref bool currentAssigned)
    {
        var rank = SeoPaymentStatuses.LifecycleRank(d.PaymentStatus);
        var cancelled = SeoPaymentStatuses.IsOffChain(d.PaymentStatus);
        var views = new List<SeoApprovalStepView>();
        var ordinal = 5;

        foreach (var (code, title) in ExecutionStages)
        {
            var stageRank = SeoPaymentStatuses.LifecycleRank(code);
            var reachedStage = headDone && !cancelled;

            var state = !reachedStage ? SeoApprovalStepState.Waiting
                : rank > stageRank ? SeoApprovalStepState.Done
                : rank == stageRank ? SeoApprovalStepState.Current
                : SeoApprovalStepState.Waiting;

            // Only one node in the whole timeline may read as "current".
            if (state == SeoApprovalStepState.Current)
            {
                if (currentAssigned)
                    state = SeoApprovalStepState.Done;
                else
                    currentAssigned = true;
            }

            views.Add(new SeoApprovalStepView(
                code, ordinal++, title,
                state == SeoApprovalStepState.Done ? "Xong"
                    : state == SeoApprovalStepState.Current ? "Đang ở đây"
                    : "Chưa tới",
                state, null, null, null, IsApprovalStep: false));
        }

        if (cancelled)
        {
            views.Add(new SeoApprovalStepView(
                d.PaymentStatus, ordinal,
                SeoRequestStatusHelper.GetPaymentStatusLabel(d.PaymentStatus),
                "Phiếu dừng ở đây", SeoApprovalStepState.Rejected,
                null, null, null, IsApprovalStep: false));
        }

        return views;
    }

    /// <summary>The step a ticket is waiting on, or null when the chain is finished or stopped.</summary>
    public static SeoApprovalStepView? FindCurrent(IEnumerable<SeoApprovalStepView> steps) =>
        steps.FirstOrDefault(s => s.State == SeoApprovalStepState.Current);

    private static readonly Dictionary<string, string> RailLabels = new()
    {
        [SeoRequestApprovalSteps.Lead] = "Lead",
        [SeoRequestApprovalSteps.Ic] = "IC",
        [SeoRequestApprovalSteps.Assistant] = "Assist",
        [SeoRequestApprovalSteps.Head] = "Head"
    };

    private static SeoApprovalStepView Step(
        string code, int ordinal, string title, string statusLabel,
        SeoApprovalStepState state, string? actor, DateTime? actedAt, string? note) =>
        new(code, ordinal, title, statusLabel, state, actor, actedAt, note,
            ShortTitle: RailLabels.GetValueOrDefault(code));

    /// <summary>Approval steps only — the execution half is driven by payment_status.</summary>
    public static List<SeoApprovalStepView> ApprovalStepsOnly(IEnumerable<SeoApprovalStepView> steps) =>
        steps.Where(s => s.IsApprovalStep).ToList();

    private static SeoApprovalStepState Resolve(bool done, bool rejected, bool reached, ref bool currentAssigned)
    {
        if (done)
            return SeoApprovalStepState.Done;

        if (rejected)
            return SeoApprovalStepState.Rejected;

        if (reached && !currentAssigned)
        {
            currentAssigned = true;
            return SeoApprovalStepState.Current;
        }

        return SeoApprovalStepState.Waiting;
    }

    private static bool IsAny(string? status, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        var normalized = status.Trim().ToUpperInvariant();
        return values.Contains(normalized);
    }
}
