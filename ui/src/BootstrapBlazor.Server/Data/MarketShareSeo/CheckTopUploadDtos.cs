using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data.MarketShareSeo;

/// <summary>
/// Thống kê 1 tháng có trong file upload, do files-worker trả về.
/// </summary>
public class CheckTopMonthStatDto
{
    [JsonPropertyName("month")]
    public string? Month { get; set; }

    [JsonPropertyName("rows_in_file")]
    public int RowsInFile { get; set; }

    [JsonPropertyName("rows_in_db")]
    public int RowsInDb { get; set; }

    /// <summary>Số dòng của tháng này thực sự sẽ được thêm (chưa có trong DB).</summary>
    [JsonPropertyName("rows_new")]
    public int RowsNew { get; set; }

    /// <summary>Chế độ ghi đè: số dòng lát cắt MKT02 của tháng này trong DB.</summary>
    [JsonPropertyName("rows_mkt02_in_db")]
    public int RowsMkt02InDb { get; set; }

    /// <summary>Trong đó còn bao nhiêu dòng đang mang ô gộp "K Na".</summary>
    [JsonPropertyName("rows_kna_in_db")]
    public int RowsKnaInDb { get; set; }
}

/// <summary>
/// Một dòng mẫu trong bảng "cũ → mới" của chế độ ghi đè, để người bấm nhìn thấy
/// thay đổi cụ thể trước khi xác nhận chứ không chỉ thấy một con số tổng.
/// </summary>
public class CheckTopPicSampleDto
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    [JsonPropertyName("results")]
    public string? Results { get; set; }

    /// <summary>PIC hiện tại, ví dụ "K Na".</summary>
    [JsonPropertyName("old")]
    public string? Old { get; set; }

    /// <summary>PIC sẽ ghi, ví dụ "Joinwick".</summary>
    [JsonPropertyName("new")]
    public string? New { get; set; }

    /// <summary>
    /// Tổ con ở cột N, CHỈ để tham khảo — không ghi vào cột team của bảng dữ liệu.
    /// Tổ con suy được từ roster theo tên PIC nên không cần lưu.
    /// </summary>
    [JsonPropertyName("team_hint")]
    public string? TeamHint { get; set; }
}

/// <summary>
/// Một tab trong file, kèm việc nó có nạp được cho luồng ghi đè không.
/// Đếm số cột KHÔNG đủ làm tiêu chí: file thật có 22 tab >= 15 cột nhưng chỉ 7 tab
/// đúng cấu trúc, số còn lại là định dạng cũ 25 cột nghĩa hoàn toàn khác.
/// </summary>
public class CheckTopSheetDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>Lý do không nạp được, để hiện cho người dùng khỏi phải đoán.</summary>
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Một domain vẫn còn dòng mang ô gộp "K Na" sau lượt nạp.
/// Hệ thống KHÔNG tự đoán PIC cho các dòng này: đo trên dữ liệu thật 2026-08-29,
/// 18/21 dòng còn sót không có manh mối ở bất kỳ bảng nào — kể cả bảng sở hữu
/// domain cũng ghi "K Na". Việc gán phải làm ở đầu nguồn (sheet Phân loại).
/// </summary>
public class CheckTopKnaDomainDto
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>Số dòng của domain này còn kẹt.</summary>
    [JsonPropertyName("rows")]
    public int Rows { get; set; }
}

/// <summary>Số liệu của từng sheet trong một lượt nạp nhiều sheet.</summary>
public class CheckTopPerSheetDto
{
    [JsonPropertyName("sheet")]
    public string Sheet { get; set; } = string.Empty;

    [JsonPropertyName("override_rows")]
    public int OverrideRows { get; set; }

    [JsonPropertyName("blank_rows")]
    public int BlankRows { get; set; }
}

/// <summary>
/// Kết quả gọi files-worker (preview hoặc commit).
/// </summary>
public class CheckTopUploadResultDto
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("flow")]
    public string? Flow { get; set; }

    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [JsonPropertyName("months")]
    public List<CheckTopMonthStatDto> Months { get; set; } = new();

    /// <summary>Tổng số dòng đọc được từ sheet (kể cả dòng rỗng).</summary>
    [JsonPropertyName("total_rows")]
    public int TotalRows { get; set; }

    /// <summary>Dòng rỗng hoàn toàn bị bỏ qua (file thật ~27.400 dòng).</summary>
    [JsonPropertyName("skipped_empty")]
    public int SkippedEmpty { get; set; }

    /// <summary>Dòng thiếu cột bắt buộc hoặc vượt độ dài — bị bỏ.</summary>
    [JsonPropertyName("skipped_invalid")]
    public int SkippedInvalid { get; set; }

    [JsonPropertyName("invalid_samples")]
    public List<string> InvalidSamples { get; set; } = new();

    [JsonPropertyName("valid_rows")]
    public int ValidRows { get; set; }

    /// <summary>Số dòng sẽ được thêm — đã trừ dòng đã tồn tại trong DB.</summary>
    [JsonPropertyName("rows_new")]
    public int RowsNew { get; set; }

    /// <summary>Số dòng bị bỏ qua vì đã có trong DB (chống nạp trùng, không xoá gì).</summary>
    [JsonPropertyName("skipped_existing")]
    public int SkippedExisting { get; set; }

    /// <summary>Số dòng đã ghi (chỉ có ở mode=commit).</summary>
    [JsonPropertyName("inserted")]
    public int? Inserted { get; set; }

    // ── Chế độ ghi đè PIC (action=fix-pic) ───────────────────────────────────
    // Đường nạp thường không trả các field này; chúng mặc định 0/null nên không
    // ảnh hưởng gì tới màn hình cũ.

    /// <summary>"append" hoặc "fix-pic".</summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>Số dòng trong file có điền đủ cột N/O — tức là muốn đổi.</summary>
    [JsonPropertyName("file_override_rows")]
    public int FileOverrideRows { get; set; }

    /// <summary>Dòng bỏ trống N/O — không muốn đổi, giữ nguyên.</summary>
    [JsonPropertyName("rows_blank")]
    public int RowsBlank { get; set; }

    /// <summary>Trong số bỏ trống, bao nhiêu dòng còn kẹt trong ô gộp "K Na".</summary>
    [JsonPropertyName("rows_kna_blank")]
    public int RowsKnaBlank { get; set; }

    /// <summary>Số dòng sẽ bị ghi đè team/pic.</summary>
    [JsonPropertyName("rows_will_change")]
    public int RowsWillChange { get; set; }

    /// <summary>Đã mang đúng team/pic đích rồi (chạy lại cùng file lần hai).</summary>
    [JsonPropertyName("rows_already_correct")]
    public int RowsAlreadyCorrect { get; set; }

    /// <summary>
    /// Không tìm thấy dòng khớp khoá (ngày, keyword, results) trong lát cắt MKT02.
    /// Gồm cả dòng thuộc team khác — những dòng đó nằm ngoài phạm vi được sửa.
    /// </summary>
    [JsonPropertyName("rows_not_found_in_db")]
    public int RowsNotFoundInDb { get; set; }

    /// <summary>Số dòng trong DB vẫn mang ô gộp "K Na" sau lượt nạp này.</summary>
    [JsonPropertyName("rows_kna_remaining")]
    public int RowsKnaRemaining { get; set; }

    /// <summary>Các domain còn kẹt, nhiều dòng nhất lên đầu.</summary>
    [JsonPropertyName("kna_domains")]
    public List<CheckTopKnaDomainDto> KnaDomains { get; set; } = new();

    /// <summary>Số dòng đã ghi đè (chỉ có ở mode=commit).</summary>
    [JsonPropertyName("rows_updated")]
    public int? RowsUpdated { get; set; }

    /// <summary>Mã lượt ghi đè — cần để hoàn tác, phải hiện cho người dùng thấy.</summary>
    [JsonPropertyName("batch_id")]
    public string? BatchId { get; set; }

    /// <summary>Số liệu tách theo từng sheet khi nạp nhiều sheet một lượt.</summary>
    [JsonPropertyName("per_sheet")]
    public List<CheckTopPerSheetDto> PerSheet { get; set; } = new();

    /// <summary>Tối đa 20 dòng mẫu "cũ → mới".</summary>
    [JsonPropertyName("samples")]
    public List<CheckTopPicSampleDto> Samples { get; set; } = new();

    [JsonPropertyName("duration_sec")]
    public double DurationSec { get; set; }

    /// <summary>HTTP status của lần gọi — UI dùng để phân biệt 409 (trùng tháng) với 423 (đang bận).</summary>
    [JsonIgnore]
    public int StatusCode { get; set; }
}
