namespace BootstrapBlazor.Server.Data;

public class ChatActionRequest
{
    public string Action { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Dictionary<string, string>? Params { get; set; }
}
