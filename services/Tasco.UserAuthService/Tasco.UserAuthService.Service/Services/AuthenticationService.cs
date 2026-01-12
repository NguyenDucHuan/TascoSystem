using Azure.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        public AuthenticationService(UserManager<IdentityUser> userManager, ITokenService tokenService, IEmailService emailService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidUserIdException("User ID and token are required");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            // Decode the Base64 URL-encoded token
            byte[] decodedBytes = WebEncoders.Base64UrlDecode(token);
            string decodedToken = Encoding.UTF8.GetString(decodedBytes);

            // Confirm email with the decoded token
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                throw new EmailConfirmationFailedException();
            }

            // Unlock the account
            user.LockoutEnabled = false;
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;

            // Update the user in the database
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new UserUpdateFailedException(errors);
            }

            return true;
        }

        public async Task<AccountBusiness> LoginAsync(LoginAccountBusiness loginAccount)
        {
            var user = await _userManager.FindByEmailAsync(loginAccount.Email);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                throw new EmailNotConfirmedException();
            }

            if (user.LockoutEnabled)
            {
                throw new AccountDisabledException();
            }

            if (!await _userManager.CheckPasswordAsync(user, loginAccount.Password))
            {
                throw new InvalidCredentialsException();
            }

            var roles = await _userManager.GetRolesAsync(user) ?? new List<string>();
            var jwtId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name , user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var accessToken = _tokenService.GenerateJwtToken(user, claims);

            return new AccountBusiness
            {
                UserId = Guid.Parse(user.Id),
                Email = user.Email,
                Roles = roles.ToArray(),
                Tokens = new TokenBusiness
                {
                    AccessToken = accessToken,
                }
            };
        }

        public async Task<bool> RegisterAsync(string baseUrl, RegisterAccountBusiness registerAccount)
        {
            // Check for existing user
            var existingUser = await _userManager.FindByEmailAsync(registerAccount.Email);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException();
            }

            // Create new user
            var identityUser = new IdentityUser
            {
                UserName = registerAccount.FullName,
                Email = registerAccount.Email,
                EmailConfirmed = false,
                LockoutEnabled = false,
            };

            var identityResult = await _userManager.CreateAsync(identityUser, registerAccount.Password);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                throw new UserCreationFailedException(errors);
            }

            // Add roles if specified
            if (registerAccount.RoleId?.Any() == true)
            {
                identityResult = await _userManager.AddToRolesAsync(identityUser, registerAccount.RoleId);
                if (!identityResult.Succeeded)
                {
                    var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                    throw new RoleAssignmentFailedException(errors);
                }
            }

            // Generate and send email confirmation
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationLink = $"{baseUrl.TrimEnd('/')}?userId={identityUser.Id}&token={encodedToken}";

            var confirmEmailBusiness = new ConfirmEmailBusiness
            {
                Email = identityUser.Email,
                ConfirmationLink = confirmationLink
            };

            await _emailService.SendConfirmationEmailAsync(confirmEmailBusiness);

            return true;
        }
    }
}
