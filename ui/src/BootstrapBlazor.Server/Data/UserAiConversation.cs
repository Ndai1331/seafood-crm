namespace BootstrapBlazor.Server.Data;

/// <summary>
/// User AI conversation thread
/// </summary>
public class UserAiConversation
{
    public long Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4.5";
    public int TotalMessages { get; set; } = 0;
    public DateTime? LastMessageAt { get; set; }
    public bool IsPinned { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<UserAiMessage> Messages { get; set; } = new List<UserAiMessage>();
}
