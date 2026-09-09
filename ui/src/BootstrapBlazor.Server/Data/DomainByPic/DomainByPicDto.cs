using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BootstrapBlazor.Server.Data;
public class DomainUniverSheetDto
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = "Paste vào ô này để thêm miền";
    [JsonPropertyName("team")]
    public string Team { get; set; }
    [JsonPropertyName("pic")]
    public string Pic { get; set; }
}


public class DomainCheckerDto
{
    public string Domain { get; set; } = string.Empty;
    public string RequestUrl { get; set; } = string.Empty;
    public string RedirectLogs { get; set; } = string.Empty;
    public string RedirectError { get; set; } = string.Empty;
    public string FinalDomain { get; set; } = string.Empty;
    public int RedirectCount { get; set; } = 0;
    public string RedirectChain { get; set; } = string.Empty;
    public List<RedirectHopDto> RedirectHops { get; set; } = new();
}

public class RedirectHopDto
{
    public int StepNumber { get; set; }
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string RedirectType { get; set; } = string.Empty;
    public string NextUrl { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
}

public class RedirectCheckResult
{
    public string FinalUrl { get; set; } = string.Empty;
    public string Logs { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int RedirectCount { get; set; }
    public List<RedirectHopDto> Hops { get; set; } = new();

    public string Chain => string.Join(" → ", Hops.Select(hop => $"{hop.StatusCode}: {hop.Url}"));
}

public class PicTeamOptionsDto
{
    [JsonPropertyName("pics")]
    public List<string> Pics { get; set; } = new();
    [JsonPropertyName("teams")]
    public List<string> Teams { get; set; } = new();
}

public class DomainSearchDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("domain")]
    public string Domain { get; set; }
    [JsonPropertyName("team")]
    public string Team { get; set; }
    [JsonPropertyName("pic")]
    public string Pic { get; set; }
    [JsonPropertyName("backgroundColorTeam")]
    public string? BackgroundColorTeam { get; set; }
    [JsonPropertyName("textColorTeam")]
    public string? TextColorTeam { get; set; }
    [JsonPropertyName("domainAge")]
    public int? DomainAge { get; set; }
    [JsonPropertyName("backgroundColorPic")]
    public string? BackgroundColorPic { get; set; }
    [JsonPropertyName("textColorPic")]
    public string? TextColorPic { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("isActive")]
    public int IsActive { get; set; } = 1;
    [NotMapped]
    [JsonPropertyName("isMainSite")]
    public bool IsMainSite { get; set; }
}


public class CreateUpdateDomainSearchDto
{
    [Required(ErrorMessage = "Domain is required")]
    [StringLength(255, ErrorMessage = "Domain cannot exceed 255 characters")]
    public string Domain { get; set; }

    [Required(ErrorMessage = "Team is required")]
    [StringLength(255, ErrorMessage = "Team cannot exceed 255 characters")]
    public string Team { get; set; }

    [Required(ErrorMessage = "Pic is required")]
    [StringLength(255, ErrorMessage = "Pic cannot exceed 255 characters")]
    public string Pic { get; set; }

    public string BackgroundColorTeam { get; set; }
    public string TextColorTeam { get; set; }
    public string BackgroundColorPic { get; set; }
    public string TextColorPic { get; set; }
    public int IsActive { get; set; } = 1;
}

public class DomainSearchFilterPagingDto : FilterPagingBase
{
    public string? Domain { get; set; }
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
public class DomainSearchHistoryDto
{
    public int Id { get; set; }
    public string Domain { get; set; }
    public string Team { get; set; }
    public string Pic { get; set; }
    public string? NewPic { get; set; }
    public string? NewTeam { get; set; }
    public string? NewDomain { get; set; }
    public string? Action { get; set; }
    public string? Source { get; set; }
    public string? ChangedBy { get; set; }
    public string? BatchId { get; set; }
    public string? BackgroundColorTeam { get; set; }
    public string? TextColorTeam { get; set; }
    public string? BackgroundColorPic { get; set; }
    public string? TextColorPic { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Vietnamese label for the change source, shown in the history modal.</summary>
    public string SourceLabel => Source switch
    {
        "ui_import" => "Nhập dữ liệu (dán sheet)",
        "ui_edit" => "Sửa trên web",
        "ui_bulk_edit" => "Sửa hàng loạt",
        "sheet_sync" => "Đồng bộ tự động từ file",
        "request_sync" => "Từ phiếu duyệt mua",
        _ => "Không rõ nguồn"
    };

    /// <summary>Vietnamese label for the action type.</summary>
    public string ActionLabel => Action switch
    {
        "create" => "Thêm mới",
        "update" => "Cập nhật",
        "deactivate" => "Đánh dấu không dùng",
        "delete" => "Xoá",
        _ => "Cập nhật"
    };
}

public class DomainSearchFilterDto: ITableSearchModel
{
    public string? Domain { get; set; }
    public string? Team { get; set; }
    public string? Pic { get; set; }
    public IEnumerable<IFilterAction> GetSearches()
    {
        var ret = new List<IFilterAction>();
        return ret;
    }

    public void Reset()
    {
        Domain = null;
        Team = null;
        Pic = null;
    }
}
