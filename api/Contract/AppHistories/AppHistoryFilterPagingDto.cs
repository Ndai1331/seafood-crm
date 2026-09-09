namespace Contract.AppHistories
{
    public class AppHistoryFilterPagingDto : BaseFilterPagingDto
    {
        public DateTime? Date { set; get; }
        public string? IpAddress { get; set; }
        public string? Functions { get; set; }
        public string? Operation { get; set; }
        public string? FullName { get; set; }
        public int? UserId { get; set; }
    }
}
