namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Audit trail for permission changes
/// </summary>
public class UserAiSkillAuditLog
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "granted" | "revoked"
    public long? PerformedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public User? PerformedByUser { get; set; }
}
