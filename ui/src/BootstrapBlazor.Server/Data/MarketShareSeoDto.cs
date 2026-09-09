namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Market Share SEO DTO for API response
/// </summary>
public class MarketShareSeoDto
{
    /// <summary>
    /// Team name (e.g., "MKT02", "Competitors")
    /// </summary>
    public string Team { get; set; } = string.Empty;

    /// <summary>
    /// Total top 10 count
    /// </summary>
    public int TotalTop10 { get; set; }

    /// <summary>
    /// Ratio total percentage
    /// </summary>
    public double? RatioTotal { get; set; }

    /// <summary>
    /// Ratio internal percentage
    /// </summary>
    public double? RatioInternal { get; set; }
}

