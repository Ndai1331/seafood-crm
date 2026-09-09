namespace BootstrapBlazor.Server.Data;

/// <summary>
/// User AI skill DTO for API responses
/// </summary>
public class UserAiSkillDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public long? GrantedBy { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Computed properties
    public string GrantedByName { get; set; } = string.Empty;
}

/// <summary>
/// Request to grant skill to user
/// </summary>
public class GrantSkillRequest
{
    public long UserId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public long GrantedBy { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Request to revoke skill from user
/// </summary>
public class RevokeSkillRequest
{
    public long UserId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public long RevokedBy { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Request to grant multiple skills at once
/// </summary>
public class BulkGrantSkillsRequest
{
    public long UserId { get; set; }
    public List<string> SkillNames { get; set; } = new();
    public long GrantedBy { get; set; }
}

/// <summary>
/// Audit log entry DTO
/// </summary>
public class UserAiSkillAuditLogDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "granted" | "revoked"
    public long? PerformedBy { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }

    // Computed properties
    public string PerformedByName { get; set; } = string.Empty;
}
