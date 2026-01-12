using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Tasco.Gateway.API.Helper;
using Yarp.ReverseProxy.Transforms;

namespace Tasco.Gateway.API.Middlewares
{
    public static class TokenTransformExtensions
    {
        public static IReverseProxyBuilder AddUserClaimsTransforms(this IReverseProxyBuilder proxyBuilder)
        {
            proxyBuilder.AddTransforms(context =>
            {
                context.AddRequestTransform(async transformContext =>
                {
                    var payload = JwtHelper.GetDecodedPayload(transformContext.HttpContext.Request, out string errorMessage);

                    if (payload != null)
                    {
                        // Helper: lấy claim an toàn từ payload
                        string? GetClaim(string key) =>
                            payload.TryGetValue(key, out var value) && value != null ? value.ToString() : null;

                        // Lấy các claim phổ biến
                        var userId = GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                        var email = GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                        var role = GetClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                        var name = GetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

                        // Add vào headers
                        if (!string.IsNullOrEmpty(userId))
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-UserId", userId);
                        if (!string.IsNullOrEmpty(email))
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-UserEmail", email);
                        if (!string.IsNullOrEmpty(role))
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-UserRoles", role);
                        if (!string.IsNullOrEmpty(name))
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-UserName", name);

                        // Add tất cả các claim còn lại
                        foreach (var kvp in payload)
                        {
                            var claimKey = kvp.Key;
                            var claimValue = kvp.Value?.ToString() ?? "";

                            // Đừng thêm lại các claim đã thêm ở trên
                            if (claimKey.Contains("emailaddress") || claimKey.Contains("nameidentifier") ||
                                claimKey.Contains("role") || claimKey.Contains("name")) continue;

                            var headerKey = "X-Claim-" + claimKey.Split('/').Last(); // ví dụ: X-Claim-emailaddress
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation(headerKey, claimValue);
                        }

                        // Thay thế {claim:...} trong route
                        string path = transformContext.Path.Value ?? string.Empty;

                        path = Regex.Replace(path, @"\{claim:([^}]+)\}", match =>
                        {
                            var shortKey = match.Groups[1].Value.ToLower();

                            // Map key ngắn sang full claim URI
                            var claimMap = new Dictionary<string, string>
                            {
                                ["id"] = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                                ["nameidentifier"] = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                                ["userid"] = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                                ["email"] = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                                ["role"] = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                                ["name"] = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                            };

                            string fullClaimKey = claimMap.TryGetValue(shortKey, out var mapped) ? mapped : shortKey;

                            return GetClaim(fullClaimKey) ?? match.Value;
                        });

                        if (path != transformContext.Path.Value)
                        {
                            transformContext.Path = path;
                        }
                    }
                });
            });

            return proxyBuilder;
        }
    }


}