namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Conversation item with date group label for sidebar list
/// </summary>
public class ConversationListDto
{
    public string ConversationId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Model { get; set; } = "claude-sonnet-4.5";
    public int TotalMessages { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    /// <summary>Group label: "Today", "Yesterday", "Last 7 days", "Older"</summary>
    public string GroupLabel { get; set; } = string.Empty;
}

/// <summary>
/// Paginated response for conversation list
/// </summary>
public class ConversationListResponse
{
    public List<ConversationListDto> Conversations { get; set; } = new();
    public int Total { get; set; }
    public bool HasMore { get; set; }
}

/// <summary>
/// Request body to rename a conversation
/// </summary>
public class RenameConversationDto
{
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Request body to pin/unpin a conversation
/// </summary>
public class PinConversationDto
{
    public bool IsPinned { get; set; }
}
