using Domain.AppConfigs;
using Domain.Identity.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.AppHistories
{
    [Table("apphistories")]
    public class AppHistory
    {
        public AppHistory() 
        { 
            Date = DateTime.Now;
        }
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { set; get; }
        [MaxLength(255)]
        public string IpAddress { get; set; }
        public string Functions { get; set; }
        public string Operation { get; set; }
        [MaxLength(512)]
        public string? UserAgent { get; set; }
        [MaxLength(32)]
        public string? DeviceType { get; set; }
        [MaxLength(128)]
        public string? Browser { get; set; }
        [MaxLength(128)]
        public string? OperatingSystem { get; set; }
        [MaxLength(64)]
        public string? RequestId { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public bool Succeeded { get; set; }
        public User User { get; set; }
    }
}
