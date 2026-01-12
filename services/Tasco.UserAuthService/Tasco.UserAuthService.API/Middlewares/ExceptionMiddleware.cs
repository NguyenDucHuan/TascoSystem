using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Tasco.UserAuthService.Service.Exceptions;
using Tasco.UserAuthService.API.Models.ResponseModels;

namespace Tasco.UserAuthService.API.Middlewares
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
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            var errorId = Guid.NewGuid();
            var (statusCode, message) = GetErrorResponse(exception);

            // Log the exception with appropriate level
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                logger.LogError(exception, $"{errorId} : {exception.Message}");
            }
            else
            {
                logger.LogWarning(exception, $"{errorId} : {exception.Message}");
            }

            // Create error response
            var errorResponse = ApiResponse<object>.ErrorResponse(
                message,
                new List<string> { exception.Message }
            );

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(errorResponse);
        }

        private static (HttpStatusCode statusCode, string message) GetErrorResponse(Exception exception)
        {
            return exception switch
            {
                // Authentication related exceptions (400 Bad Request)
                UserNotFoundException => (HttpStatusCode.BadRequest, "Authentication failed"),
                InvalidCredentialsException => (HttpStatusCode.BadRequest, "Authentication failed"),
                EmailNotConfirmedException => (HttpStatusCode.BadRequest, "Email verification required"),
                AccountDisabledException => (HttpStatusCode.BadRequest, "Account access denied"),
                EmailAlreadyExistsException => (HttpStatusCode.BadRequest, "Registration failed"),
                InvalidTokenException => (HttpStatusCode.BadRequest, "Invalid verification token"),
                InvalidUserIdException => (HttpStatusCode.BadRequest, "Invalid request parameters"),
                
                // User operation failures (422 Unprocessable Entity)
                UserCreationFailedException => (HttpStatusCode.UnprocessableEntity, "User registration failed"),
                RoleAssignmentFailedException => (HttpStatusCode.UnprocessableEntity, "Role assignment failed"),
                EmailConfirmationFailedException => (HttpStatusCode.UnprocessableEntity, "Email confirmation failed"),
                UserUpdateFailedException => (HttpStatusCode.UnprocessableEntity, "User update failed"),
                
                // Configuration issues (503 Service Unavailable)
                ConfigurationMissingException => (HttpStatusCode.ServiceUnavailable, "Service configuration error"),
                
                // File/Resource not found (404 Not Found)
                EmailTemplateNotFoundException => (HttpStatusCode.NotFound, "Email template not found"),
                EmailLogoNotFoundException => (HttpStatusCode.NotFound, "Email assets not found"),
                
                // Service failures (502 Bad Gateway)
                EmailSendFailedException => (HttpStatusCode.BadGateway, "Email service temporarily unavailable"),
                TokenGenerationFailedException => (HttpStatusCode.BadGateway, "Authentication service error"),
                
                // Default case (500 Internal Server Error)
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };
        }
    }
}
