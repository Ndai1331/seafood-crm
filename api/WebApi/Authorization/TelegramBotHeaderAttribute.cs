using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Authorization;

public class TelegramBotHeaderAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private const string RequiredHeaderKey = "telegram-bot";
    private const string RequiredHeaderValue = "QXH!d4htzCmP";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(RequiredHeaderKey, out var headerValue))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (headerValue != RequiredHeaderValue)
        {
            context.Result = new UnauthorizedResult();
        }
    }
} 