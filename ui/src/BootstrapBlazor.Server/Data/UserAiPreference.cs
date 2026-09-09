namespace BootstrapBlazor.Server.Data;

/// <summary>
/// User-specific AI chatbot preferences
/// </summary>
public class UserAiPreference
{
    public long UserId { get; set; }
    public string DefaultModel { get; set; } = "claude-sonnet-4.5";
    public int ContextWindow { get; set; } = 200000;
    public decimal Temperature { get; set; } = 1.00m;
    public bool AutoSave { get; set; } = true;
    public bool ShowTokens { get; set; } = false;
    public string Theme { get; set; } = "auto"; // "light" | "dark" | "auto"
    public string Language { get; set; } = "vi-VN";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
}
