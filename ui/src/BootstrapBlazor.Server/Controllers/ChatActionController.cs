using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BootstrapBlazor.Server.Data;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace BootstrapBlazor.Server.Controllers;

/// <summary>Internal endpoint for whitelisted task actions — called from ChatWidget AI context</summary>
[ApiController]
[Route("api/internal/chat/action")]
public class ChatActionController : ControllerBase
{
    private readonly ILogger<ChatActionController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    private static readonly HashSet<string> AllowedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "get_my_keywords",
        "get_payment_status",
        "get_domain_health",
    };

    public ChatActionController(
        ILogger<ChatActionController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("execute")]
    [AllowAnonymous]
    public async Task<IActionResult> Execute([FromBody] ChatActionRequest request)
    {
        if (!AllowedActions.Contains(request.Action))
        {
            _logger.LogWarning("ChatAction: blocked action '{Action}'", request.Action);
            return BadRequest(new { error = $"Action '{request.Action}' không được phép." });
        }

        _logger.LogInformation("ChatAction: executing '{Action}' for user {UserId}",
            request.Action, request.UserId);

        var baseUrl = _configuration["RemoteServices:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
        var client = _httpClientFactory.CreateClient();

        var result = request.Action.ToLowerInvariant() switch
        {
            "get_my_keywords"    => await FetchAsync(client, $"{baseUrl}/api/keywords?userId={request.UserId}&pageSize=5"),
            "get_payment_status" => await FetchAsync(client, $"{baseUrl}/api/payments?userId={request.UserId}&pageSize=5"),
            "get_domain_health"  => await FetchAsync(client, $"{baseUrl}/api/domain-check?domain={Uri.EscapeDataString(request.Params?.GetValueOrDefault("domain") ?? "")}"),
            _                    => (object)"Không có kết quả."
        };

        return Ok(new { result });
    }

    private static async Task<object> FetchAsync(HttpClient client, string url)
    {
        try
        {
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return $"API trả về lỗi {(int)response.StatusCode}.";
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception)
        {
            return "Không thể kết nối đến API.";
        }
    }
}
