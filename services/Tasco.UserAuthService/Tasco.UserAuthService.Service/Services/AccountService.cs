using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tasco.UserAuthService.Service.BusinessModels;
using Tasco.UserAuthService.Service.Exceptions;
using Tasco.UserAuthService.Service.Services.Interface;

namespace Tasco.UserAuthService.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AccountBusiness> GetAccountInfoAsync(ClaimsPrincipal userClaims)
        {
            if (userClaims == null)
            {
                throw new InvalidTokenException("User claims are required");
            }

            // Extract user ID from claims
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new InvalidTokenException("User ID not found in claims");
            }

            // Find user by ID
            var user = await _userManager.FindByIdAsync(userIdClaim.Value);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();

            return new AccountBusiness
            {
                UserId = Guid.Parse(user.Id),
                Email = user.Email ?? string.Empty,
                Roles = roles.ToArray()
            };
        }
    }
} 