namespace BootstrapBlazor.Server.Data;

/// <summary>
/// User AI session entity (database model)
/// Tracks active Claude session state for multi-user isolation
/// </summary>
public class UserAiSession
{
    /// <summary>
    /// User ID (primary key, one session per user)
    /// </summary>
    public ulong UserId { get; set; }

    /// <summary>
    /// SHA-256 hash of session ID (64 hex chars) - for query/index
    /// </summary>
    public string SessionIdHash { get; set; } = string.Empty;

    /// <summary>
    /// AES-256-CBC encrypted session ID (for resume after restart)
    /// Format: base64(iv + ciphertext), ~88 chars
    /// </summary>
    public string SessionIdEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// Optional conversation ID linkage (references user_ai_conversations.conversation_id)
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// Workspace directory path (e.g. /workspaces/123)
    /// </summary>
    public string WorkspacePath { get; set; } = string.Empty;

    /// <summary>
    /// Session creation timestamp (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last activity timestamp (UTC, updated on each request)
    /// </summary>
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for creating new session (POST /api/user-ai-sessions)
/// </summary>
public class CreateUserAiSessionDto
{
    public ulong UserId { get; set; }
    public string SessionIdHash { get; set; } = string.Empty;
    public string SessionIdEncrypted { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public string WorkspacePath { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating session (PUT /api/user-ai-sessions/:userId)
/// </summary>
public class UpdateUserAiSessionDto
{
    public string? ConversationId { get; set; }
    public DateTime? LastActive { get; set; }
}

/// <summary>
/// DTO for batch delete request (DELETE /api/user-ai-sessions/batch)
/// </summary>
public class BatchDeleteSessionsDto
{
    public List<ulong> UserIds { get; set; } = [];
}
