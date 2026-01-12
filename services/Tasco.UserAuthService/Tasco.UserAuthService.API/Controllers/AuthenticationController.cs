using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Tasco.UserAuthService.Service.Services.Interface;
using Tasco.UserAuthService.Service.BusinessModels;
using Tasco.UserAuthService.API.Models.RequestModels;
using Tasco.UserAuthService.API.Models.ResponseModels;

namespace Tasco.UserAuthService.API.Controllers
{
    [ApiController]
    [Route("api/authentications")]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationController(
            IAuthenticationService authenticationService,
            IMapper mapper,
            ILogger<AuthenticationController> logger,
            IConfiguration configuration)
        {
            _authenticationService = authenticationService;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>Login response with user information and access token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var loginBusiness = _mapper.Map<LoginAccountBusiness>(request);
            var account = await _authenticationService.LoginAsync(loginBusiness);

            var response = _mapper.Map<LoginResponse>(account);
            
            _logger.LogInformation("User logged in successfully: {UserId}", account.UserId);
            
            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
        }

        /// <summary>
        /// User registration endpoint
        /// </summary>
        /// <param name="request">Registration information</param>
        /// <returns>Registration confirmation</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 422)]
        [ProducesResponseType(typeof(ApiResponse<object>), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            var registerBusiness = _mapper.Map<RegisterAccountBusiness>(request);
            var baseUrl = $"{Request.Scheme}://{Request.Host}/api/authentications/confirm-email";
            
            await _authenticationService.RegisterAsync(baseUrl, registerBusiness);

            var response = new RegisterResponse();
            
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            
            return Ok(ApiResponse<RegisterResponse>.SuccessResponse(response, "Registration successful"));
        }

        /// <summary>
        /// Email confirmation endpoint - redirects to frontend with status
        /// </summary>
        /// <param name="userId">User ID from email confirmation link</param>
        /// <param name="token">Confirmation token from email</param>
        /// <returns>Redirect to frontend with confirmation status</returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
            var confirmationPath = _configuration["Frontend:EmailConfirmationPath"];
            
            if (string.IsNullOrEmpty(frontendBaseUrl))
            {
                frontendBaseUrl = "http://localhost:3000"; // Default fallback
            }
            
            if (string.IsNullOrEmpty(confirmationPath))
            {
                confirmationPath = "/auth/email-confirmation"; // Default fallback
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Email confirmation failed: Missing userId or token");
                var errorUrl = $"{frontendBaseUrl}{confirmationPath}?status=error&message=Invalid confirmation link. Please check your email and try again.";
                return Redirect(errorUrl);
            }

            try
            {
                _logger.LogInformation("Email confirmation attempt for user: {UserId}", userId);

                await _authenticationService.ConfirmEmailAsync(userId, token);
                
                _logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);
                
                // Redirect to frontend success page
                var successUrl = $"{frontendBaseUrl}{confirmationPath}?status=success&message=Email confirmed successfully! You can now login to your account.";
                return Redirect(successUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email confirmation failed for user: {UserId}", userId);
                
                // Redirect to frontend error page
                var errorMessage = Uri.EscapeDataString("Email confirmation failed. The link may be expired or invalid. Please try registering again.");
                var errorUrl = $"{frontendBaseUrl}{confirmationPath}?status=error&message={errorMessage}";
                return Redirect(errorUrl);
            }
        }

      
    }
} 