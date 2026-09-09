namespace BootstrapBlazor.Server.Data;

public class MarketShareSeoDetailFilterDto
{
    public string FromYearMonth { get; set; } = string.Empty;
    public string ToYearMonth { get; set; } = string.Empty;
    public string? Keyword { get; set; }
    public string Team { get; set; } = "MKT02";
    public string? Classification1 { get; set; }
    public string? Pic { get; set; }
    public string? Results { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = 1000;
}
