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
        public User User { get; set; }
    }
}