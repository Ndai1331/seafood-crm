namespace BootstrapBlazor.Server.Data;

public class KeywordDto
{
    public long Id { get; set; }
    public int BrandId { get; set; }
    public string KeywordText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? LatestVolume { get; set; }
}

public class KeywordWithUserCountDto 
{
    public long Id { get; set; }
    public int BrandId { get; set; }
    public string KeywordText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int UserCount { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public bool HasRankingData { get; set; } = false;
    public bool HasCostData { get; set; } = false;
    public List<string> UserNames { get; set; } = new List<string>();
    public List<MonthlyVolumeDto> MonthlyVolumes { get; set; } = new List<MonthlyVolumeDto>();
}

public class CreateUpdateKeywordDto
{
    public int BrandId { get; set; }
    public string KeywordText { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class MonthlyVolumeDto
{
    public string YearMonth { get; set; } = string.Empty; // "2025-01"
    public int SearchVolume { get; set; }
}

/// <summary>
/// DTO for checking keywords that do not exist
/// </summary>
public class CheckKeywordsRequestDto
{
    public List<string> Keywords { get; set; } = new List<string>();
}

/// <summary>
/// DTO for response of checking keywords that do not exist
/// </summary>
public class CheckKeywordsResponseDto
{
    public List<string> NotExistsKeywords { get; set; } = new List<string>();
     public bool HasNotExistsKeywords { get; set; } = false;
    public List<KeywordDto> Keywords { get; set; } = new List<KeywordDto>();
}
