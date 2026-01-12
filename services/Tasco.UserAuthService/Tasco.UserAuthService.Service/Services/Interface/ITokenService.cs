using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.UserAuthService.Service.Services.Interface
{
    public interface ITokenService
    {
        string GenerateJwtToken(IdentityUser user, List<Claim> claims);
        ClaimsPrincipal? ValidateJwtToken(string token);
    }
}
