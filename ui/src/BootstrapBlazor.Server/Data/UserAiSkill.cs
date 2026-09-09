namespace BootstrapBlazor.Server.Data;

/// <summary>
/// User AI skill permissions
/// </summary>
public class UserAiSkill
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public long? GrantedBy { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public User? GrantedByUser { get; set; }
}
