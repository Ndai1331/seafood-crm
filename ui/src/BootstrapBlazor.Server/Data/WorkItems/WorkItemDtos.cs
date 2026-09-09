namespace BootstrapBlazor.Server.Data.WorkItems;

public static class WorkItemStatuses
{
    public const string New = "New";
    public const string Todo = "Todo";
    public const string Doing = "Doing";
    public const string Review = "Review";
    public const string Done = "Done";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<WorkItemOption> All =
    [
        new(New, "Mới", "fa-solid fa-inbox"),
        new(Todo, "Cần làm", "fa-solid fa-list-check"),
        new(Doing, "Đang làm", "fa-solid fa-person-running"),
        new(Review, "Đang duyệt", "fa-solid fa-magnifying-glass-check"),
        new(Done, "Hoàn thành", "fa-solid fa-circle-check"),
        new(Cancelled, "Đã hủy", "fa-solid fa-ban")
    ];

    public static WorkItemOption Get(string? value) =>
        All.FirstOrDefault(option => option.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
        ?? All[0];
}

public static class WorkItemPriorities
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Urgent = "Urgent";

    public static readonly IReadOnlyList<WorkItemOption> All =
    [
        new(Low, "Thấp", "fa-solid fa-arrow-down"),
        new(Medium, "Trung bình", "fa-solid fa-minus"),
        new(High, "Cao", "fa-solid fa-arrow-up"),
        new(Urgent, "Khẩn cấp", "fa-solid fa-triangle-exclamation")
    ];

    public static WorkItemOption Get(string? value) =>
        All.FirstOrDefault(option => option.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
        ?? All[1];
}

public static class WorkItemDueStates
{
    public static readonly IReadOnlyList<WorkItemOption> All =
    [
        new("Overdue", "Quá hạn", "fa-solid fa-clock"),
        new("Today", "Hôm nay", "fa-solid fa-calendar-day"),
        new("Upcoming", "Sắp tới", "fa-solid fa-calendar"),
        new("NoDueDate", "Chưa có hạn", "fa-regular fa-calendar-xmark")
    ];
}

public sealed record WorkItemOption(string Value, string Label, string Icon);

public sealed class WorkItemFilterDto
{
    public string? Search { get; set; }
    public int? TeamId { get; set; }
    public int? AssigneeId { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? DueState { get; set; }
    public DateTime? StartDateFrom { get; set; }
}

public sealed class WorkItemListResponseDto
{
    public List<WorkItemDto> Items { get; set; } = [];
    public Dictionary<string, int> LaneCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int TotalCount { get; set; }
    public bool IsTruncated { get; set; }
}

public sealed class WorkItemDto
{
    public long Id { get; set; }
    public string Code => $"TASK-{Id}";
    public string Title { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Priority { get; set; } = WorkItemPriorities.Medium;
    public string Status { get; set; } = WorkItemStatuses.New;
    public int RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string? TeamCode { get; set; }
    public string? TeamName { get; set; }
    public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanAssign { get; set; }
}

public class CreateWorkItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Priority { get; set; } = WorkItemPriorities.Medium;
    public int? TeamId { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public sealed class UpdateWorkItemDto : CreateWorkItemDto
{
    public int ExpectedVersion { get; set; }
}

public sealed class ChangeWorkItemStatusDto
{
    public string ToStatus { get; set; } = string.Empty;
    public int ExpectedVersion { get; set; }
}

public sealed class WorkItemActivityDto
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? FromStatus { get; set; }
    public string? ToStatus { get; set; }
    public string? ChangedFields { get; set; }
    public int? ActorId { get; set; }
    public string ActorName { get; set; } = "System";
    public DateTime CreatedAt { get; set; }
}

public sealed class WorkItemAssigneeOptionDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? UserCode { get; set; }
    public int? TeamId { get; set; }
    public string? PositionName { get; set; }
}

public sealed class WorkItemTeamOptionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
}
