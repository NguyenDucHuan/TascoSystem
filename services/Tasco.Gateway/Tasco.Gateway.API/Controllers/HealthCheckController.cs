using Microsoft.AspNetCore.Mvc;

namespace Tasco.Gateway.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Status = "Gateway is running", Timestamp = DateTime.UtcNow });
        }
        [HttpGet("token-info")]
        public IActionResult GetTokenInfo()
        {
            var authHeader = Request.Headers.Authorization.ToString();

            return Ok(new
            {
                IsAuthenticated = User?.Identity?.IsAuthenticated,
                Claims = User?.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList(),
                AuthHeader = string.IsNullOrEmpty(authHeader) ? null :
                    (authHeader.Length > 20 ? $"{authHeader.Substring(0, 20)}..." : authHeader)
            });
        }
    }
}