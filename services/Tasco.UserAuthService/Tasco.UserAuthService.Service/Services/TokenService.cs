using Azure.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tasco.UserAuthService.Service.Services.Interface;
using Tasco.UserAuthService.Service.Exceptions;

namespace Tasco.UserAuthService.Service.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GenerateJwtToken(IdentityUser user, List<Claim> claims)
        {
            // Validate configuration values
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ConfigurationMissingException("Jwt:Key");
            }
            
            if (string.IsNullOrEmpty(jwtIssuer))
            {
                throw new ConfigurationMissingException("Jwt:Issuer");
            }
            
            if (string.IsNullOrEmpty(jwtAudience))
            {
                throw new ConfigurationMissingException("Jwt:Audience");
            }

            try
            {
                // Create security key and signing credentials
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Generate JWT token
                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.Now.AddDays(100),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new TokenGenerationFailedException(ex.Message, ex);
            }
        }

        public ClaimsPrincipal? ValidateJwtToken(string token)
        {
            // Validate configuration values
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ConfigurationMissingException("Jwt:Key");
            }

            if (string.IsNullOrEmpty(jwtIssuer))
            {
                throw new ConfigurationMissingException("Jwt:Issuer");
            }

            if (string.IsNullOrEmpty(jwtAudience))
            {
                throw new ConfigurationMissingException("Jwt:Audience");
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
