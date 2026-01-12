using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Tasco.UserAuthService.Service.Services.Interface;
using Tasco.UserAuthService.Service.BusinessModels;
using Tasco.UserAuthService.API.Models.RequestModels;
using Tasco.UserAuthService.API.Models.ResponseModels;

namespace Tasco.UserAuthService.API.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            IMapper mapper,
            ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get account information from access token in Authorization header
        /// </summary>
        /// <returns>Account information including user details and roles</returns>
        [HttpPost("get-info")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AccountResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> GetAccountInfo()
        {
            _logger.LogInformation("Get account info attempt for user: {UserId}", User.Identity?.Name);

            var account = await _accountService.GetAccountInfoAsync(User);

            var response = _mapper.Map<AccountResponse>(account);
            
            _logger.LogInformation("Account info retrieved successfully: {UserId}", account.UserId);
            
            return Ok(ApiResponse<AccountResponse>.SuccessResponse(response, "Account information retrieved successfully"));
        }
    }
} 