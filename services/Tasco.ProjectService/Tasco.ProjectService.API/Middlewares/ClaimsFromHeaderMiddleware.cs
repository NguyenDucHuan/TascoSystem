using System.Security.Claims;

namespace Tasco.ProjectService.API.Middlewares
{
    public class ClaimsFromHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClaimsFromHeaderMiddleware> _logger;

        public ClaimsFromHeaderMiddleware(RequestDelegate next, ILogger<ClaimsFromHeaderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Đọc thông tin người dùng từ headers
            if (context.Request.Headers.TryGetValue("X-UserId", out var userId))
            {
                _logger.LogInformation("Nhận thông tin người dùng từ Gateway: UserId={UserId}", userId);

                var identity = new ClaimsIdentity("GatewayAuth");
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

                if (context.Request.Headers.TryGetValue("X-UserEmail", out var email))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, email));
                }

                if (context.Request.Headers.TryGetValue("X-UserRoles", out var roles))
                {
                    foreach (var role in roles.ToString().Split(','))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }

                if (context.Request.Headers.TryGetValue("X-UserName", out var name))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, name));
                }

                // Thêm các claims khác từ headers
                foreach (var header in context.Request.Headers.Where(h => h.Key.StartsWith("X-Claim-")))
                {
                    string claimType = header.Key.Substring(8);
                    identity.AddClaim(new Claim(claimType, header.Value));
                }

                context.User = new ClaimsPrincipal(identity);
            }

            await _next(context);
        }
    }

    // Extension method để dễ dàng thêm vào pipeline
    public static class ClaimsMiddlewareExtensions
    {
        public static IApplicationBuilder UseClaimsFromHeaders(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsFromHeaderMiddleware>();
        }
    }
}