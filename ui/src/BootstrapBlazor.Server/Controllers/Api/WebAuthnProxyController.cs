using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

namespace BootstrapBlazor.Server.Controllers.Api;

/// <summary>
/// Same-origin forwarder for the WebAuthn ceremony endpoints.
///
/// It exists because of a hard browser constraint: navigator.credentials only responds when it is
/// called inside a real user gesture. Blazor Server handles the click on the server and calls back
/// into JS over SignalR, by which point the gesture is gone and the call hangs forever with no
/// dialog and no error. So the ceremony has to be driven from JS inside the click handler — and
/// JS cannot reach the API's internal address (task9-api-test:8080), only this origin.
///
/// Deliberately narrow: it forwards nothing except the webauthn/ prefix, so it cannot be used as a
/// general tunnel to the API. Authorization is passed straight through and never inspected here —
/// the API remains the only thing deciding who the caller is.
/// </summary>
[ApiController]
[Microsoft.AspNetCore.Mvc.Route("webauthn-proxy")]
public class WebAuthnProxyController : ControllerBase
{
    private static readonly string[] AllowedPaths =
    {
        "register/begin", "register/complete",
        "enroll/begin", "enroll/complete",
        "login/begin", "login/complete"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebAuthnProxyController> _logger;

    public WebAuthnProxyController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WebAuthnProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("{*path}")]
    public async Task<IActionResult> ForwardAsync(string path)
    {
        if (!AllowedPaths.Contains(path))
        {
            return NotFound();
        }

        var baseUrl = _configuration["RemoteServices:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogError("RemoteServices:BaseUrl is not configured — cannot forward a WebAuthn ceremony");
            return StatusCode(500);
        }

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/webauthn/{path}")
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
        };

        if (AuthenticationHeaderValue.TryParse(Request.Headers.Authorization.ToString(), out var auth))
        {
            request.Headers.Authorization = auth;
        }

        try
        {
            using var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = responseBody,
                ContentType = "application/json"
            };
        }
        catch (Exception ex)
        {
            // Without this the unhandled exception is rendered to the caller complete with the
            // API's internal host and a stack trace.
            _logger.LogError(ex, "Could not reach the API to forward webauthn/{Path}", path);
            return new ContentResult
            {
                StatusCode = 502,
                Content = "{\"error\":\"Không kết nối được máy chủ xác thực.\"}",
                ContentType = "application/json"
            };
        }
    }
}
