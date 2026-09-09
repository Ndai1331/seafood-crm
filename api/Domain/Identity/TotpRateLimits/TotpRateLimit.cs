namespace Domain.Identity.TotpRateLimits
{
    public class TotpRateLimit
    {
        public int Id { get; set; }
        public string? IpAddress { get; set; }
        public int? UserId { get; set; }
        public int FailureCount { get; set; }
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;
        public DateTime? LockedUntil { get; set; }
    }
}
