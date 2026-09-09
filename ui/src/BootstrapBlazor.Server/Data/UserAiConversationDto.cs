namespace BootstrapBlazor.Server.Data;

/// <summary>
/// Conversation DTO
/// </summary>
public class UserAiConversationDto
{
    public long Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TotalMessages { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Message DTO
/// </summary>
public class UserAiMessageDto
{
    public long Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" | "assistant" | "system"
    public string Content { get; set; } = string.Empty;
    public int? Tokens { get; set; }
    public string? Metadata { get; set; } // JSON string
    public int Sequence { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create conversation
/// </summary>
public class CreateConversationRequest
{
    public long UserId { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Request to add message
/// </summary>
public class AddMessageRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? Tokens { get; set; }
    public string? Metadata { get; set; }
}

/// <summary>
/// Request to update conversation title
/// </summary>
public class UpdateTitleRequest
{
    public string ConversationId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Request to search conversations
/// </summary>
public class SearchConversationsRequest
{
    public long UserId { get; set; }
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
}

/// <summary>
/// Conversation statistics
/// </summary>
public class ConversationStatsDto
{
    public int TotalConversations { get; set; }
    public int TotalMessages { get; set; }
    public int TotalTokens { get; set; }
    public DateTime? LastChatAt { get; set; }
}

/// <summary>
/// Paginated result wrapper
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
