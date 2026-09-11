using Application.AppHistorys;
using Contract.AppHistories;
using System.Diagnostics;

namespace WebApi.Middlewares
{
    /// <summary>
    /// Records authenticated API requests with client context so the system can answer who did
    /// what, from where and with which device. The audit endpoint itself is excluded to avoid noise.
    /// </summary>
    public class AppHistoryMiddleware
    {
        private readonly RequestDelegate _next;

        public AppHistoryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                await WriteHistoryAsync(context, stopwatch.ElapsedMilliseconds);
            }
        }

        private static async Task WriteHistoryAsync(HttpContext context, long durationMs)
        {
            if (context.Request.Path.StartsWithSegments("/api/appHistories", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var primarySid = context.User.Claims?.FirstOrDefault(x =>
                x.Type.EndsWith("/primarysid", StringComparison.OrdinalIgnoreCase)
                || x.Type.Equals("sub", StringComparison.OrdinalIgnoreCase)
                || x.Type.Equals("userId", StringComparison.OrdinalIgnoreCase))?.Value;

            if (string.IsNullOrWhiteSpace(primarySid)
                || !int.TryParse(primarySid, out var userId)
                || string.IsNullOrWhiteSpace(context.Request.Method))
            {
                return;
            }

            var appHistoryService = context.RequestServices.GetService<AppHistoryService>();
            if (appHistoryService == null)
            {
                return;
            }

            var request = context.Request;
            var ipAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var userAgent = request.Headers["X-Client-UserAgent"].ToString();
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                userAgent = request.Headers.UserAgent.ToString();
            }

            var client = ClientInfo.Parse(userAgent);
            await appHistoryService.CreateAsync(new CreateUpdateAppHistoryDto
            {
                Date = DateTime.Now,
                UserId = userId,
                IpAddress = ipAddress,
                Operation = request.Method,
                Functions = request.Path.ToString(),
                UserAgent = Truncate(userAgent, 512),
                DeviceType = client.DeviceType,
                Browser = client.Browser,
                OperatingSystem = client.OperatingSystem,
                RequestId = Truncate(context.TraceIdentifier, 64),
                StatusCode = context.Response.StatusCode,
                DurationMs = durationMs,
                Succeeded = context.Response.StatusCode is >= 200 and < 400
            });
        }

        private static string? Truncate(string? value, int maxLength)
            => string.IsNullOrWhiteSpace(value) ? null : value.Length <= maxLength ? value : value[..maxLength];

        private sealed record ClientInfo(string DeviceType, string Browser, string OperatingSystem)
        {
            public static ClientInfo Parse(string? userAgent)
            {
                var ua = userAgent ?? string.Empty;
                var device = ua.Contains("iPad", StringComparison.OrdinalIgnoreCase)
                    || ua.Contains("Tablet", StringComparison.OrdinalIgnoreCase) ? "Tablet"
                    : ua.Contains("Mobile", StringComparison.OrdinalIgnoreCase)
                        || ua.Contains("Android", StringComparison.OrdinalIgnoreCase) ? "Mobile" : "Desktop";
                var browser = ua.Contains("Edg/", StringComparison.OrdinalIgnoreCase) ? "Edge"
                    : ua.Contains("OPR/", StringComparison.OrdinalIgnoreCase) ? "Opera"
                    : ua.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) ? "Chrome"
                    : ua.Contains("Firefox/", StringComparison.OrdinalIgnoreCase) ? "Firefox"
                    : ua.Contains("Safari/", StringComparison.OrdinalIgnoreCase) ? "Safari" : "Other";
                var operatingSystem = ua.Contains("Windows", StringComparison.OrdinalIgnoreCase) ? "Windows"
                    : ua.Contains("Mac OS", StringComparison.OrdinalIgnoreCase) ? "macOS"
                    : ua.Contains("Android", StringComparison.OrdinalIgnoreCase) ? "Android"
                    : ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase) ? "iOS"
                    : ua.Contains("Linux", StringComparison.OrdinalIgnoreCase) ? "Linux" : "Other";
                return new ClientInfo(device, browser, operatingSystem);
            }
        }
    }
}
