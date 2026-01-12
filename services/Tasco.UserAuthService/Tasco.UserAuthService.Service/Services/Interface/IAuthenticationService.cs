using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.UserAuthService.Service.BusinessModels;

namespace Tasco.UserAuthService.Service.Services.Interface
{
    public interface IAuthenticationService
    {
        Task<AccountBusiness> LoginAsync(LoginAccountBusiness loginAccount);
        Task<bool> RegisterAsync(string baseUrl, RegisterAccountBusiness registerAccount);
        Task<bool> ConfirmEmailAsync(string userId, string token);
    }
}
