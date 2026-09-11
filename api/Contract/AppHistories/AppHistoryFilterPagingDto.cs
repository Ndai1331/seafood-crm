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
        public string? Search { get; set; }
        public string? DeviceType { get; set; }
        public string? Browser { get; set; }
        public string? OperatingSystem { get; set; }
        public bool? Succeeded { get; set; }
    }
}
