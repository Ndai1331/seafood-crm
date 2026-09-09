using Newtonsoft.Json;

namespace BootstrapBlazor.Server.Data.Wiki
{
    public class WikiAutoPageListItem
    {
        [JsonProperty("pageType")] public string PageType { get; set; } = "domain";
        [JsonProperty("entityKey")] public string EntityKey { get; set; } = string.Empty;
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("slug")] public string Slug { get; set; } = string.Empty;
        [JsonProperty("domainRating")] public decimal? DomainRating { get; set; }
        [JsonProperty("organicTraffic")] public int? OrganicTraffic { get; set; }
        [JsonProperty("rankedKeywords")] public int? RankedKeywords { get; set; }
        [JsonProperty("bestRank")] public int? BestRank { get; set; }
        [JsonProperty("refreshedAt")] public DateTime RefreshedAt { get; set; }
    }

    public class WikiDomainDetail
    {
        [JsonProperty("domain")] public string Domain { get; set; } = string.Empty;
        [JsonProperty("domainRating")] public decimal? DomainRating { get; set; }
        [JsonProperty("organicTraffic")] public int? OrganicTraffic { get; set; }
        [JsonProperty("trafficValueUsd")] public decimal? TrafficValueUsd { get; set; }
        [JsonProperty("backlinksTotal")] public int? BacklinksTotal { get; set; }
        [JsonProperty("referringDomains")] public int? ReferringDomains { get; set; }
        [JsonProperty("ahrefsSnapshotDate")] public DateTime? AhrefsSnapshotDate { get; set; }
        [JsonProperty("rankedKeywordCount")] public int RankedKeywordCount { get; set; }
        [JsonProperty("trafficTrend")] public List<WikiMetricTrendPoint> TrafficTrend { get; set; } = new();
        [JsonProperty("topKeywords")] public List<WikiRelatedKeyword> TopKeywords { get; set; } = new();
        [JsonProperty("referencingNotes")] public List<WikiReferencingNote> ReferencingNotes { get; set; } = new();
    }

    public class WikiKeywordDetail
    {
        [JsonProperty("keyword")] public string Keyword { get; set; } = string.Empty;
        [JsonProperty("volume")] public int? Volume { get; set; }
        [JsonProperty("bestRank")] public int BestRank { get; set; }
        [JsonProperty("domainCount")] public int DomainCount { get; set; }
        [JsonProperty("latestRankingDate")] public DateTime? LatestRankingDate { get; set; }
        [JsonProperty("rankTrend")] public List<WikiMetricTrendPoint> RankTrend { get; set; } = new();
        [JsonProperty("rankingDomains")] public List<WikiRelatedDomain> RankingDomains { get; set; } = new();
        [JsonProperty("referencingNotes")] public List<WikiReferencingNote> ReferencingNotes { get; set; } = new();
    }

    public class WikiReferencingNote
    {
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("slug")] public string Slug { get; set; } = string.Empty;
        [JsonProperty("scope")] public string Scope { get; set; } = "team";
    }

    public class WikiMetricTrendPoint
    {
        [JsonProperty("date")] public DateTime Date { get; set; }
        [JsonProperty("value")] public decimal Value { get; set; }
    }

    public class WikiRelatedKeyword
    {
        [JsonProperty("keyword")] public string Keyword { get; set; } = string.Empty;
        [JsonProperty("rank")] public int Rank { get; set; }
        [JsonProperty("volume")] public int Volume { get; set; }
    }

    public class WikiRelatedDomain
    {
        [JsonProperty("domain")] public string Domain { get; set; } = string.Empty;
        [JsonProperty("rank")] public int Rank { get; set; }
    }

    public class WikiAutoGraph
    {
        [JsonProperty("nodes")] public List<WikiAutoGraphNode> Nodes { get; set; } = new();
        [JsonProperty("edges")] public List<WikiAutoGraphEdge> Edges { get; set; } = new();
    }

    public class WikiAutoGraphNode
    {
        [JsonProperty("id")] public string Id { get; set; } = string.Empty;
        [JsonProperty("name")] public string Name { get; set; } = string.Empty;
        [JsonProperty("type")] public string Type { get; set; } = "domain";
        [JsonProperty("entityKey")] public string EntityKey { get; set; } = string.Empty;
        [JsonProperty("weight")] public int Weight { get; set; }
        [JsonProperty("isMainsite")] public bool IsMainsite { get; set; }
        [JsonProperty("siteType")] public string SiteType { get; set; } = "keyword";
        [JsonProperty("team")] public string? Team { get; set; }
        [JsonProperty("pic")] public string? Pic { get; set; }
    }

    public class WikiAutoGraphEdge
    {
        [JsonProperty("source")] public string Source { get; set; } = string.Empty;
        [JsonProperty("target")] public string Target { get; set; } = string.Empty;
    }
}
