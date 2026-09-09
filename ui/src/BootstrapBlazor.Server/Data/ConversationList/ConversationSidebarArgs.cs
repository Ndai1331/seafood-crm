namespace BootstrapBlazor.Server.Data;

/// <summary>Args passed when a conversation is renamed from the sidebar.</summary>
public record ConversationRenameArgs(string ConversationId, string NewTitle);
