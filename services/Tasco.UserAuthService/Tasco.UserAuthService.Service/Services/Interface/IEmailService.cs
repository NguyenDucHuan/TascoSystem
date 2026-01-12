using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.UserAuthService.Service.BusinessModels;

namespace Tasco.UserAuthService.Service.Services.Interface
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(ConfirmEmailBusiness request);
    }
}
