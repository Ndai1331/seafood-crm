namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Per-user markdown file metadata
/// </summary>
public class UserAiFile
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FilePath { get; set; } = string.Empty; // Relative path: {user_id}/{filename}.md
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; } = 0;
    public string? FileHash { get; set; } // SHA256
    public string? ConversationId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? SyncedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public UserAiConversation? Conversation { get; set; }
}
