namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Market Share Report Response DTO - wraps the data array with additional metadata
/// </summary>
public class MarketShareReportResponseDto
{
    /// <summary>
    /// List of market share data
    /// </summary>
    public List<MarketShareSeoDto> Data { get; set; } = new();

    /// <summary>
    /// From month (format: MM-yyyy)
    /// </summary>
    public string? FromMonth { get; set; }

    /// <summary>
    /// To month (format: MM-yyyy)
    /// </summary>
    public string? ToMonth { get; set; }

    /// <summary>
    /// Total all count
    /// </summary>
    public int? TotalAll { get; set; }

    /// <summary>
    /// Total internal count
    /// </summary>
    public int? TotalInternal { get; set; }
}
