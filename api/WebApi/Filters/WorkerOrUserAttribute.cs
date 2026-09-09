using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters
{
    /// <summary>
    /// Cho phép HAI kiểu người gọi trên cùng một endpoint:
    ///
    ///  * files-worker trên do-122 — máy, không giữ được JWT của người dùng, nên dùng khoá bí mật
    ///    ở header <c>X-Worker-Key</c> (cấu hình <c>Worker:ApiKey</c>).
    ///  * Người dùng đã đăng nhập — nút chạy tay trên giao diện.
    ///
    /// Không cấu hình khoá thì đường của worker đóng lại, KHÔNG mở toang: người dùng vẫn gọi được
    /// bằng JWT, còn worker bị từ chối cho tới khi khoá được đặt.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WorkerOrUserAttribute : Attribute, IAsyncActionFilter
    {
        public const string HeaderName = "X-Worker-Key";
        private const string ConfigKey = "Worker:ApiKey";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                await next();
                return;
            }

            var configuration = context.HttpContext.RequestServices
                .GetService(typeof(IConfiguration)) as IConfiguration;
            var expected = configuration?[ConfigKey];

            if (!string.IsNullOrWhiteSpace(expected)
                && context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
                && ApiKeyComparer.FixedTimeEquals(provided.ToString(), expected))
            {
                await next();
                return;
            }

            context.Result = new UnauthorizedResult();
        }
    }
}
