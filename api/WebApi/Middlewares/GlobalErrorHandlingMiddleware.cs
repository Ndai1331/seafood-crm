using Core.Const;
using Core.Exceptions;
using Core.Helper;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApi.Middlewares
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public static IConfiguration _configuration;

        public GlobalErrorHandlingMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _configuration = config;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            var stackTrace = string.Empty;
            string message = "";

            var exceptionType = exception.GetType();

            if (exceptionType == typeof(GlobalException))
            {
                var globalException = (GlobalException)exception;
                message = globalException.Message;
                status = globalException.Status;
                stackTrace = globalException.StackTrace;
            }
            else if (exceptionType == typeof(DbUpdateConcurrencyException))
            {
                message = HttpMessage.Conflict;
                status = HttpStatusCode.Conflict;
                stackTrace = exception.StackTrace;
            }
            else if (exceptionType == typeof(DbUpdateException))
            {
                message = HttpMessage.ServerError;
                status = HttpStatusCode.InternalServerError;
                stackTrace = exception.StackTrace;
                FileHelper.WriteLog(exception, _configuration["Media:LOG_PATH"]);
            }
            else if (exception is DbException)
            {
                // The raw provider message names databases, tables and columns, and this handler
                // also covers anonymous endpoints such as user/sign-in. Log it, do not ship it.
                message = HttpMessage.ServerError;
                status = HttpStatusCode.BadGateway;
                stackTrace = exception.StackTrace;
                FileHelper.WriteLog(exception, _configuration["Media:LOG_PATH"]);
            }
            else
            {
                message = HttpMessage.ServerError;
                status = HttpStatusCode.InternalServerError;
                stackTrace = exception.StackTrace;
                FileHelper.WriteLog(exception, _configuration["Media:LOG_PATH"]);
            }

            var includeStackTrace = _configuration.GetValue<bool>("DetailedErrors");
            var exceptionResult = JsonSerializer.Serialize(includeStackTrace
                ? new { error = message, stackTrace }
                : new { error = message, stackTrace = string.Empty });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            return context.Response.WriteAsync(exceptionResult);
        }
    }
}