using Domain.Identity.Users;

namespace Domain.AppConfigs
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
        public User? CreatedUser { get; set; }
        public int? ModifiedBy { get; set; }
        public User? ModifiedUser { get; set; }
    }
}