using Tasco.UserAuthService.Service.BusinessModels;

namespace Tasco.UserAuthService.API.Models.ResponseModels
{
    public class AccountResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = new string[0];
    }
}
