using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Keyword stats item for a PIC.
/// </summary>
public class UserKeywordStatsDto
{
    [JsonPropertyName("keywordText")]
    public string KeywordText { get; set; } = string.Empty;

    [JsonPropertyName("declarationDate")]
    public DateTime? DeclarationDate { get; set; }

    [JsonPropertyName("top1Count")]
    public int Top1Count { get; set; }

    [JsonPropertyName("top2Count")]
    public int Top2Count { get; set; }

    [JsonPropertyName("top3Count")]
    public int Top3Count { get; set; }

    [JsonPropertyName("top4Count")]
    public int Top4Count { get; set; }

    [JsonPropertyName("top5Count")]
    public int Top5Count { get; set; }

    [JsonPropertyName("top6Count")]
    public int Top6Count { get; set; }

    [JsonPropertyName("top7Count")]
    public int Top7Count { get; set; }

    [JsonPropertyName("top8Count")]
    public int Top8Count { get; set; }

    [JsonPropertyName("top9Count")]
    public int Top9Count { get; set; }

    [JsonPropertyName("top10Count")]
    public int Top10Count { get; set; }

    [JsonPropertyName("hasRankingData")]
    public bool HasRankingData { get; set; }
}

/// <summary>
/// Response wrapper for user keyword stats report.
/// </summary>
public class UserKeywordStatsResponseDto
{
    [JsonPropertyName("data")]
    public List<UserKeywordStatsDto> Data { get; set; } = new();

    [JsonPropertyName("userId")]
    public int? UserId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("fromDate")]
    public DateTime? FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateTime? ToDate { get; set; }
}
