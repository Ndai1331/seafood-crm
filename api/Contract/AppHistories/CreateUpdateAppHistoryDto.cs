using System;

namespace Contract.AppHistories
{
    public class CreateUpdateAppHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { set; get; }
        public string IpAddress { get; set; }
        public string Functions { get; set; }
        public string Operation { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceType { get; set; }
        public string? Browser { get; set; }
        public string? OperatingSystem { get; set; }
        public string? RequestId { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public bool Succeeded { get; set; }
    }
}
