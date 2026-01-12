using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.Model;
using System.Text.Json;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Configuration;

namespace Tasco.Gateway.API.Middlewares
{
    public class GatewayAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public GatewayAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the route requires authentication
            var routeConfig = context.GetRouteConfig();
            bool requiresAuth = false;

            if (routeConfig != null &&
                routeConfig.Metadata.TryGetValue("RequiresAuth", out var requiresAuthString))
            {
                requiresAuth = requiresAuthString.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            // Specific paths that don't require auth
            var path = context.Request.Path.Value?.ToLower();

            // Allow authentication endpoints to pass through without auth
            if (path?.StartsWith("/api/authentications/login") == true ||
                path?.StartsWith("/api/authentications/register") == true ||
                path?.StartsWith("/api/authentications/confirm-email") == true)
            {
                await _next(context);
                return;
            }

            // Check if authentication is required for this route
            if (requiresAuth)
            {
                // Check if the Authorization header is present
                if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                    string.IsNullOrWhiteSpace(authHeader))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Authorization header is missing"
                    });
                    return;
                }

                // The actual validation will be done by the Authentication middleware
                // This is just an additional check to ensure the header is present
            }

            await _next(context);
        }
    }

    // Extension method for easier middleware registration
    public static class GatewayAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseGatewayAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GatewayAuthMiddleware>();
        }
    }

    // Extension method to get the proxy feature with correct routing information
    public static class HttpContextExtensions
    {
        public static RouteConfig GetRouteConfig(this HttpContext context)
        {
            // Get the YARP IReverseProxyFeature which has the routing information
            var proxyFeature = context.Features.Get<IReverseProxyFeature>();
            if (proxyFeature == null)
            {
                return null;
            }

            // Return the route configuration
            return proxyFeature.Route?.Config;
        }
    }
}