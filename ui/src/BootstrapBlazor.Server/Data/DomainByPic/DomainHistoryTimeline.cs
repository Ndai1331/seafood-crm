namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Người dùng chỉ quan tâm hai câu hỏi: domain vào hệ thống lúc nào, và nó đã
/// qua tay những ai. Bảng lịch sử thô trả lời cả hai nhưng chôn chúng dưới hàng
/// nghìn dòng vô nghĩa — 4.428 dòng "tạo mới" cho vỏn vẹn 472 domain, và 6.040
/// dòng cập nhật không hề đổi PIC.
/// </summary>
public enum DomainHistoryEventKind
{
    /// <summary>Domain lần đầu vào danh sách task9 — tức đã mua xong.</summary>
    Acquired,

    /// <summary>PIC đổi từ người này sang người khác.</summary>
    PicTransfer,

    /// <summary>Domain bị đánh dấu không dùng.</summary>
    Deactivated,

    /// <summary>
    /// Người thật sửa gì đó không phải PIC — đổi team, mở lại domain.
    /// <para>
    /// Trước đây những dòng này bị lược mất vì lọc chỉ giữ dòng đổi PIC, nên
    /// thao tác tắt domain của đồng nghiệp không hiện ở đâu cả.
    /// </para>
    /// </summary>
    OtherEdit
}

/// <summary>Một sự kiện đáng hiển thị, đã rút gọn từ nhiều dòng lịch sử thô.</summary>
public class DomainHistoryEventDto
{
    public DomainHistoryEventKind Kind { get; set; }

    /// <summary>PIC trước đó. Null khi chưa từng ghi nhận ai phụ trách.</summary>
    public string? FromPic { get; set; }

    public string? ToPic { get; set; }
    public string? Team { get; set; }
    public string? Source { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public string KindLabel => Kind switch
    {
        DomainHistoryEventKind.Acquired => "Vào hệ thống",
        DomainHistoryEventKind.PicTransfer => "Chuyển PIC",
        DomainHistoryEventKind.Deactivated => "Tắt domain",
        _ => "Sửa khác"
    };

    /// <summary>Nguồn đã cập nhật dòng này: sync, dán bảng, hay thao tác trên web.</summary>
    public string SourceLabel => Source switch
    {
        "ui_import" => "Dán vào bảng",
        "ui_edit" => "Sửa trên web",
        "ui_bulk_edit" => "Sửa hàng loạt trên web",
        "sheet_sync" => "Đồng bộ từ file",
        "request_sync" => "Từ phiếu duyệt mua",
        _ => "Trước 27/07 (không rõ nguồn)"
    };

    public string SourceIcon => Source switch
    {
        "ui_import" => "fa-solid fa-paste",
        "ui_edit" or "ui_bulk_edit" => "fa-solid fa-pen",
        "sheet_sync" => "fa-solid fa-rotate",
        "request_sync" => "fa-solid fa-cart-shopping",
        _ => "fa-solid fa-clock-rotate-left"
    };

    /// <summary>Sync máy và thao tác người cần phân biệt được ngay bằng mắt.</summary>
    public bool IsAutomated => Source is "sheet_sync" or "request_sync";
}

/// <summary>
/// Rút các dòng lịch sử thô thành dòng thời gian chỉ gồm hai loại sự kiện.
/// </summary>
public static class DomainHistoryTimeline
{
    /// <param name="hiddenCount">
    /// Số dòng bị lược. Luôn hiển thị con số này cho người dùng: vụ ghi nhầm
    /// "srule" cho thấy che bớt lịch sử mà không nói là cách đánh mất sự thật.
    /// </param>
    public static List<DomainHistoryEventDto> Build(
        IEnumerable<DomainSearchHistoryDto>? rows, out int hiddenCount)
    {
        hiddenCount = 0;
        var events = new List<DomainHistoryEventDto>();
        if (rows is null) return events;

        var ordered = rows.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToList();
        string? currentPic = null;
        var acquiredSeen = false;

        foreach (var row in ordered)
        {
            // Dòng mới ghi rõ chuyển giao ở NewPic; dòng cũ chỉ chụp lại PIC tại
            // thời điểm đó, nên phải so với PIC đang giữ để suy ra chuyển giao.
            var hasExplicitTransfer = !string.IsNullOrWhiteSpace(row.NewPic);
            var from = hasExplicitTransfer ? Clean(row.Pic) : currentPic;
            var to = hasExplicitTransfer ? Clean(row.NewPic) : Clean(row.Pic);

            // Domain vào hệ thống: chỉ lần "tạo mới" ĐẦU TIÊN mới là sự kiện thật.
            // Sync lặp lại lệnh tạo cho cùng một domain hàng trăm lần.
            if (row.Action == "create" && !acquiredSeen)
            {
                acquiredSeen = true;
                currentPic = to;
                events.Add(new DomainHistoryEventDto
                {
                    Kind = DomainHistoryEventKind.Acquired,
                    ToPic = to,
                    Team = Clean(row.NewTeam) ?? Clean(row.Team),
                    Source = row.Source,
                    ChangedBy = row.ChangedBy,
                    CreatedAt = row.CreatedAt
                });
                continue;
            }

            var picUnchanged = to is null || string.Equals(from, to, StringComparison.OrdinalIgnoreCase);

            // Việc người thật làm thì luôn hiện, kể cả khi PIC không đổi: tắt một
            // domain hay đổi team là thao tác có người chịu trách nhiệm, và giấu nó
            // đi khiến "ai đã sửa dữ liệu trên web" không tra được ở đâu. Chỉ dòng
            // no-op của máy mới bị lược — sync cũ để lại hàng trăm dòng mỗi domain.
            if (picUnchanged && !IsHumanEdit(row.Source))
            {
                hiddenCount++;
                continue;
            }

            if (!picUnchanged) currentPic = to;
            events.Add(new DomainHistoryEventDto
            {
                Kind = KindOf(row.Action, picUnchanged),
                FromPic = from,
                ToPic = to,
                Team = Clean(row.NewTeam) ?? Clean(row.Team),
                Source = row.Source,
                ChangedBy = row.ChangedBy,
                CreatedAt = row.CreatedAt
            });
        }

        events.Reverse(); // mới nhất lên đầu
        return events;
    }

    /// <summary>
    /// Nhãn sự kiện suy từ hành động, không suy từ việc PIC có đổi hay không.
    /// Một lần tắt domain kèm đổi PIC trước đây hiện thành "Chuyển PIC" — đúng một
    /// nửa, và là nửa không quan trọng.
    /// </summary>
    private static DomainHistoryEventKind KindOf(string? action, bool picUnchanged)
        => action == "deactivate"
            ? DomainHistoryEventKind.Deactivated
            : picUnchanged ? DomainHistoryEventKind.OtherEdit : DomainHistoryEventKind.PicTransfer;

    /// <summary>Nguồn do người thao tác trên web, đối lập với sync máy.</summary>
    private static bool IsHumanEdit(string? source)
        => source is "ui_edit" or "ui_import" or "ui_bulk_edit";

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
