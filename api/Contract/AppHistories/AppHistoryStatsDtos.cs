namespace Contract.AppHistories
{
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
}
