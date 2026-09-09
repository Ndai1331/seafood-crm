using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters
{
    /// <summary>
    /// Shared-secret guard for machine callers that cannot hold a user JWT — currently the
    /// Google Apps Script ETL. The key lives in configuration (SheetSync:ApiKey); when it is
    /// not configured the endpoint stays closed rather than falling open.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SheetSyncApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        public const string HeaderName = "X-Sheet-Sync-Key";
        private const string ConfigKey = "SheetSync:ApiKey";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configured = context.HttpContext.RequestServices
                .GetService(typeof(IConfiguration)) as IConfiguration;
            var expected = configured?[ConfigKey];

            if (string.IsNullOrWhiteSpace(expected))
            {
                context.Result = new ObjectResult("Sheet sync is not configured on this server.")
                {
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
                || !ApiKeyComparer.FixedTimeEquals(provided.ToString(), expected))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }

    }
}
