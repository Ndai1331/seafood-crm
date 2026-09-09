namespace BootstrapBlazor.Server.Data;

public class KeywordMonthlyMetricDto
{
    public long KeywordId { get; set; }
    public string YearMonth { get; set; } = string.Empty;
    public int SearchVol { get; set; }
}

public class CreateUpdateKeywordMonthlyMetricDto
{
    public long KeywordId { get; set; }
    public string YearMonth { get; set; } = string.Empty;
    public int SearchVol { get; set; }
}

/// <summary>
/// Request DTO for importing keyword monthly metrics from Excel
/// Excel file should have 3 columns: Brand (col 1), Keyword (col 2), SearchVolume (col 3)
/// </summary>
public class ImportKeywordMonthlyMetricRequest
{
    public string YearMonth { get; set; } = string.Empty; // Format: "YYYY-MM" e.g. "2025-01"
    public byte[] ExcelBytes { get; set; } = null!;
    public string? FileName { get; set; }
}


/// <summary>Nạp file volume dạng ma trận (keyword × nhiều cột tháng) trong một lượt.</summary>
public class ImportVolumeMatrixRequest
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();

    /// <summary>Tên file gốc — quyết định đọc theo .csv hay .xlsx.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>false = chỉ đếm, không ghi.</summary>
    public bool Commit { get; set; }
}

public class VolumeMatrixMonthDto
{
    public string YearMonth { get; set; } = string.Empty;
    public int Keywords { get; set; }
    public int ExistingRows { get; set; }
}

public class VolumeMatrixImportResultDto
{
    public bool Commit { get; set; }
    public List<VolumeMatrixMonthDto> Months { get; set; } = new();
    public int TotalCells { get; set; }
    public int KeywordsCreated { get; set; }
    public List<string> KeywordsWithoutBrand { get; set; } = new();
}
