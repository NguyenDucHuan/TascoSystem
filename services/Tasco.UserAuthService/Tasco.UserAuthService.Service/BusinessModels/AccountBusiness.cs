using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.UserAuthService.Service.BusinessModels
{
    public class AccountBusiness
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = new string[0];
        public TokenBusiness Tokens { get; set; } = null!;
    }
}
