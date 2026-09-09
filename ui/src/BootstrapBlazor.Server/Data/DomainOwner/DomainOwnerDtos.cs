namespace BootstrapBlazor.Server.Data;

/// <summary>One domain and who owns it. Mirrors the API contract of the same name.</summary>
public class DomainOwnerDto
{
    public int Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public string DomainNorm { get; set; } = string.Empty;
    public int? PicUserId { get; set; }
    public string? PicName { get; set; }

    /// <summary>PIC has left the company — shown so a stale assignment is visible.</summary>
    public bool PicIsInactive { get; set; }

    /// <summary>Who held the domain before the current PIC. Empty when it never changed hands.</summary>
    public string? PreviousPicName { get; set; }

    public int? TeamId { get; set; }
    public string? TeamCode { get; set; }
    public string Status { get; set; } = "active";
    public int? DomainAge { get; set; }
    public string? Note { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedByName { get; set; }
}

public class DomainOwnerFilterDto
{
    public string? Domain { get; set; }
    public int? PicUserId { get; set; }
    public string? PicName { get; set; }
    public int? TeamId { get; set; }
    public bool? UnassignedOnly { get; set; }
    public string? Status { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;
}

public class PicOptionDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Null means the account is in no team — a data fault worth chasing, not a blank.</summary>
    public int? TeamId { get; set; }

    public string? TeamCode { get; set; }
    public bool IsInactive { get; set; }

    /// <summary>Account owns a domain, or owned one before — the only accounts worth suggesting.</summary>
    public bool IsInData { get; set; }
}

public class SetStatusDto
{
    public string DomainNorm { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}

public class AssignPicDto
{
    public string DomainNorm { get; set; } = string.Empty;
    public int? PicUserId { get; set; }

    /// <summary>Set only when someone picked a team by hand; otherwise the PIC's account decides.</summary>
    public int? TeamId { get; set; }

    public string? Note { get; set; }
}

public class TeamOptionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class ImportLineDto
{
    public string? Domain { get; set; }
    public string? Pic { get; set; }

    /// <summary>
    /// Column 2 of the assignment sheet. Kept so a three-column paste lands in the right cells,
    /// and shown for checking — but never sent to the API: team follows the person, derived from
    /// the account, so a team typed here would be contradicted by the next assignment.
    /// </summary>
    public string? Team { get; set; }
}

public class ImportApplyDto
{
    public string? RawText { get; set; }
    public List<ImportLineDto> Lines { get; set; } = new();
    public string? Note { get; set; }
}

public class ImportPreviewRowDto
{
    public string Domain { get; set; } = string.Empty;
    public string DomainNorm { get; set; } = string.Empty;
    public string? PicLabel { get; set; }
    public string Action { get; set; } = "unchanged";
    public string? TeamMismatch { get; set; }
    public string? CurrentPicName { get; set; }
    public string? NewPicName { get; set; }
    public int? NewPicUserId { get; set; }
    public string? Reason { get; set; }
}

public class ImportPreviewDto
{
    public List<ImportPreviewRowDto> Rows { get; set; } = new();
    public int CreateCount { get; set; }
    public int ReassignCount { get; set; }
    public int UnchangedCount { get; set; }
    public int UnknownPicCount { get; set; }
    public int SkippedCount { get; set; }
}

public class ImportApplyResultDto
{
    public int Created { get; set; }
    public int Reassigned { get; set; }
    public int Unchanged { get; set; }
    public int Skipped { get; set; }
    public string? BatchId { get; set; }
}

public class DomainOwnerEventDto
{
    public long Id { get; set; }
    public string DomainNorm { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? FromPicName { get; set; }
    public string? ToPicName { get; set; }
    public string? ActorName { get; set; }
    public string ActorKind { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Nhãn tiếng Việt của loại sự kiện, dùng chung cho mọi màn đọc nhật ký.</summary>
    public string EventLabel => EventType switch
    {
        "assign" => "Vào hệ thống",
        "reassign" => "Chuyển PIC",
        "archive" => "Tắt domain",
        "restore" => "Mở lại",
        "rename" => "Đổi tên miền",
        _ => EventType
    };

    /// <summary>
    /// Ai gây ra thay đổi — người hay máy. Đây là thứ cần nhìn thấy trước tiên khi đi
    /// tìm "có ai sửa tay không", nên nó có nhãn riêng thay vì để lộ mã actor_kind.
    /// </summary>
    public string ActorKindLabel => ActorKind switch
    {
        "user" => "Sửa trên web",
        "sheet" => "Đồng bộ từ file",
        "n8n" => "Từ phiếu duyệt mua",
        "migration" => "Chuyển đổi dữ liệu",
        _ => ActorKind
    };

    /// <summary>Lớp màu của badge sự kiện — bốn tông pastel, cùng bảng màu với các badge khác trên trang.</summary>
    public string EventCss => EventType switch
    {
        "assign" => "evt-in",
        "archive" => "evt-off",
        "restore" => "evt-restore",
        _ => "evt-move"
    };

    public string EventIcon => EventType switch
    {
        "assign" => "fa-solid fa-cart-shopping",
        "archive" => "fa-solid fa-ban",
        "restore" => "fa-solid fa-rotate-left",
        "rename" => "fa-solid fa-i-cursor",
        _ => "fa-solid fa-right-left"
    };

    /// <summary>Người thật, đối lập với sync máy — tô khác màu để lọc bằng mắt.</summary>
    public bool IsHuman => ActorKind == "user";

    /// <summary>Tên hiển thị của người thực hiện; máy thì lấy nhãn nguồn.</summary>
    public string ActorDisplay => string.IsNullOrWhiteSpace(ActorName) ? ActorKindLabel : ActorName!;
}

public class DomainOwnerEventFilterDto
{
    public string? DomainNorm { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorKind { get; set; }
    public DateTime? StartDay { get; set; }
    public DateTime? EndDay { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 30;
}
