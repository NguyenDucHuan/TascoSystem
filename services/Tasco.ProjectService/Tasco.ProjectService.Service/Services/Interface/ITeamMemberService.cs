using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.ProjectService.Service.Services.Interface
{
    public interface ITeamMemberService
    {

        Task<bool> AddMemberToTeamAsync(Guid teamId, string memberName);
        Task RemoveMemberFromTeamAsync(Guid teamId, string memberName);
        Task<IEnumerable<string>> GetTeamMembersAsync(Guid teamId);
    }
}
