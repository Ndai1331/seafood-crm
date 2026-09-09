namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Individual message in a conversation
/// </summary>
public class UserAiMessage
{
    public long Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" | "assistant" | "system"
    public string Content { get; set; } = string.Empty;
    public int? Tokens { get; set; }
    public string? Metadata { get; set; } // JSON string
    public int Sequence { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public UserAiConversation? Conversation { get; set; }
}
