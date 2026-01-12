using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Tasco.TaskService.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly ILogger<ExceptionHandlerMiddleware> logger;
        private readonly RequestDelegate next;

        public ExceptionMiddleware(
            ILogger<ExceptionHandlerMiddleware> logger,
            RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Processing request: {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
                }
                await next(httpContext);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();
                //log this exception
                logger.LogError(ex, $"{errorId} : {ex.Message}");
                //return custom error response
                if (ex is UnauthorizedAccessException)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    httpContext.Response.ContentType = "application/json";
                    var error = new
                    {
                        Id = errorId,
                        ErrorCode = 401,
                        ErrorMessage = ex.Message
                    };
                    await httpContext.Response.WriteAsJsonAsync(error);
                }
                else
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    httpContext.Response.ContentType = "application/json";
                    var error = new
                    {
                        Id = errorId,
                        ErrorCode = 500,
                        ErrorMessage = "Something went wrong. Please contact support."
                    };
                    await httpContext.Response.WriteAsJsonAsync(error);
                }
            }
        }
    }
}
