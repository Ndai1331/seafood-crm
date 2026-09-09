namespace BootstrapBlazor.Server.Data.KeywordRanking;

/// <summary>Yêu cầu dựng dữ liệu Báo cáo SEO tổng quan của một tháng từ sổ chốt KPI.</summary>
public class CheckTopSyncRequest
{
    /// <summary>Dạng "YYYY-MM".</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>false = xem trước, không ghi.</summary>
    public bool Commit { get; set; }
}

public class CheckTopSyncResultDto
{
    public string Month { get; set; } = string.Empty;
    public bool Commit { get; set; }
    public int SourceRows { get; set; }
    public int Inserted { get; set; }
    public int Skipped { get; set; }
    public int Invalid { get; set; }
    public int VolumesUpdated { get; set; }
    public int PicNamesUpdated { get; set; }
    public List<string> PicsUnresolved { get; set; } = new();
    public int GroupsAdded { get; set; }
    public int GroupsSkipped { get; set; }
    public int GroupKeywordsAdded { get; set; }
    public List<string> NoVolumeKeywords { get; set; } = new();

    /// <summary>Khác 0 = còn dòng mang ô gộp "K Na", nghĩa là chưa quy về người trước khi đồng bộ.</summary>
    public int KnaRemaining { get; set; }
}
