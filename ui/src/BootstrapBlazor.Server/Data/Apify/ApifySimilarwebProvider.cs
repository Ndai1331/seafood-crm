namespace BootstrapBlazor.Server.Data.Apify;

public class ApifySimilarwebProvider
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ApiToken { get; set; } = "";
    public string ActorId { get; set; } = "crawlerbros/similarweb-scraper";
    public int Priority { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public int TotalRuns { get; set; } = 100;
    public int UsedRuns { get; set; }
    public int RemainingRuns => Math.Max(0, TotalRuns - UsedRuns);
    public int FailCount { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? LastFailedAt { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ApifySimilarwebResult
{
    public string Domain { get; set; } = "";
    public int TotalVisits { get; set; }
    public double BounceRate { get; set; }
    public double PagesPerVisit { get; set; }
    public double AvgVisitDuration { get; set; }
    public int GlobalRank { get; set; }
    public int CountryRank { get; set; }
    public string TopCountriesJson { get; set; } = "";
    public DateTime? SnapshotDate { get; set; }
    public string EngagementMonth { get; set; } = "";
    public bool IsSmall { get; set; }
    public string RawJson { get; set; } = "";
}
