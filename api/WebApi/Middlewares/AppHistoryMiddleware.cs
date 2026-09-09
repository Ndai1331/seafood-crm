using Application.AppHistorys;
using Contract.AppHistories;
using System.Net;

namespace WebApi.Middlewares
{
    public class AppHistoryMiddleware
    {
        private readonly RequestDelegate _next;

        public AppHistoryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next(context);

            var primarysid = context.User.Claims?.FirstOrDefault(x => x.Type.EndsWith("/primarysid", StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.IsNullOrEmpty(primarysid) && int.TryParse(primarysid, out int userId) && !string.IsNullOrEmpty(context.Request.Method))
            {
                var appHistoryService = context.RequestServices.GetService<AppHistoryService>();

                if (appHistoryService != null)
                {
                    var request = context.Request;

                    IPAddress? remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;
                    string ipAddr = "";
                    if (remoteIpAddress != null)
                    {
                        if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                        {
                            remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                        }
                        ipAddr = remoteIpAddress.ToString();
                    }

                    CreateUpdateAppHistoryDto history = new CreateUpdateAppHistoryDto()
                    {
                        Date = DateTime.Now,
                        UserId = userId,
                        IpAddress = ipAddr,
                        Operation = request.Method,
                        Functions = request.Path
                    };

                    await appHistoryService.CreateAsync(history);
                }
            }
        }
    }
}
