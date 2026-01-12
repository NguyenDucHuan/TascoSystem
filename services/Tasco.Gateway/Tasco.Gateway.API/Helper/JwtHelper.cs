using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Tasco.Gateway.API.Helper
{
    public static class JwtHelper
    {
        /// <summary>
        /// Giải mã phần payload của JWT token từ header Authorization
        /// </summary>
        /// <param name="request">HttpRequest chứa header Authorization</param>
        /// <param name="errorMessage">Thông báo lỗi nếu xảy ra</param>
        /// <returns>Dictionary chứa các claims từ payload đã giải mã</returns>
        public static Dictionary<string, object>? GetDecodedPayload(HttpRequest request, out string errorMessage)
        {
            errorMessage = string.Empty;

            var authHeader = request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Missing or invalid Authorization header.";
                return null;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                return DecodeJwtPayload(token);
            }
            catch (Exception ex)
            {
                errorMessage = $"An error occurred while decoding token: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Decode phần payload của JWT token
        /// </summary>
        private static Dictionary<string, object> DecodeJwtPayload(string token)
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                throw new ArgumentException("Invalid JWT format.");

            string payload = parts[1];
            string jsonPayload = Base64UrlDecode(payload);

            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonPayload)
                   ?? throw new Exception("Failed to deserialize JWT payload.");
        }

        /// <summary>
        /// Base64Url decode
        /// </summary>
        private static string Base64UrlDecode(string input)
        {
            input = input.Replace('-', '+').Replace('_', '/');
            switch (input.Length % 4)
            {
                case 2: input += "=="; break;
                case 3: input += "="; break;
                case 0: break;
                default: throw new FormatException("Invalid base64url string.");
            }
            var bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
