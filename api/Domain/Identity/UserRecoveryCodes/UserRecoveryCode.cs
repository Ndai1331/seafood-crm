using Domain.Identity.Users;

namespace Domain.Identity.UserRecoveryCodes
{
    public class UserRecoveryCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string CodeHash { get; set; } = string.Empty;
        public string CodeSalt { get; set; } = string.Empty;
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }
    }
}
