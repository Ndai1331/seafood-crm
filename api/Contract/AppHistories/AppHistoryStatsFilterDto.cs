namespace Contract.AppHistories
{
    public class AppHistoryStatsFilterDto
    {
        public int Days { get; set; } = 30;
        public int Limit { get; set; } = 20;
        public int? UserId { get; set; }
    }
}
