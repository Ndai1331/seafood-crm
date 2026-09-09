namespace BootstrapBlazor.Server.Data.MarketShareSeo;

/// <summary>
/// Kết quả lượt tự quy ô gộp "K Na" về người thật, do task9-api trả về ngay sau khi nạp Checktop.
/// Gương của Contract.Task9.PicPerformance.KnaPicAutoFillResultDto bên API.
/// </summary>
public class KnaPicAutoFillResultDto
{
    /// <summary>Mã lượt — thứ duy nhất hoàn tác được lượt này.</summary>
    public string BatchId { get; set; } = string.Empty;

    public int Scanned { get; set; }
    public int Filled { get; set; }
    public int FromDomainOwner { get; set; }
    public int FromNaMemberRef { get; set; }
    public int FromHistory { get; set; }
    public int StillUnsplit { get; set; }

    public List<KnaPicAutoFillPicRowDto> ByPic { get; set; } = new();
    public List<KnaPicAutoFillUnresolvedRowDto> Unresolved { get; set; } = new();
}

public class KnaPicAutoFillPicRowDto
{
    public string Pic { get; set; } = string.Empty;
    public string? Team { get; set; }
    public int Rows { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class KnaPicAutoFillUnresolvedRowDto
{
    public string Domain { get; set; } = string.Empty;
    public int Rows { get; set; }
    public string Reason { get; set; } = string.Empty;
}
