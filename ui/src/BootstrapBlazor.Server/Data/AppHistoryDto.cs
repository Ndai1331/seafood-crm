
namespace BootstrapBlazor.Server.Data;

public class AppHistoryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { set; get; }
    public string IpAddress { get; set; }
    public string Functions { get; set; }
    public string Operation { get; set; }
    public string FullName { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? RequestId { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public bool Succeeded { get; set; }
}
public class AppHistorySearchResponseDto
{
    public int TotalItem { set; get; } = 0;
    public List<AppHistoryDto> Result { set; get; } = new List<AppHistoryDto>();
}

 public class AppHistoryFilterPagingDto : BaseFilterPagingDto
{
    public DateTime? Date { set; get; }
    public string? IpAddress { get; set; }
    public string? Functions { get; set; }
    public string? Operation { get; set; }
    public string? FullName { get; set; }
    public int? UserId { get; set; }
    public string? Search { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public bool? Succeeded { get; set; }
}

public class AppHistoryStatsFilterDto
{
    public int Days { get; set; } = 30;
    public int Limit { get; set; } = 20;
    public int? UserId { get; set; }
}

public class FeatureUsageDto
{
    public string FunctionPath { get; set; } = string.Empty;
    public string FeatureLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UserActivityStatsDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public DateTime? LastActivity { get; set; }
}

public class DailyActivityDto
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class HourlyActivityDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class ActivitySummaryDto
{
    public int TotalToday { get; set; }
    public int UniqueUsersToday { get; set; }
    public int TotalLast30Days { get; set; }
    public string MostUsedFeature { get; set; } = string.Empty;
    public string MostActiveUser { get; set; } = string.Empty;
}
