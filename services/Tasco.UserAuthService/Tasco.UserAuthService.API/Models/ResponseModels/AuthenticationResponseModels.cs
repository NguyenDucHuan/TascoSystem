namespace Tasco.UserAuthService.API.Models.ResponseModels
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
        public TokenResponse Tokens { get; set; } = new();
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class RegisterResponse
    {
        public string Message { get; set; } = "Registration successful. Please check your email to confirm your account.";
    }

    public class ConfirmEmailResponse
    {
        public string Message { get; set; } = "Email confirmed successfully. You can now log in to your account.";
    }
} 