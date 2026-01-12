using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.ProjectService.Service.Helper;

namespace Tasco.ProjectService.Service.Services.Interface
{
    public interface IProjectMemberService
    {
        Task<MethodResult<bool>> RemoveMemberFromProjectAsync(Guid projectId, Guid memberId, Guid owner);
        Task<MethodResult<bool>> UpdateApprovedStatusAsync(Guid projectId, Guid memberId, string ApprovedStatus, Guid? owner, string? gmail, string? userName);
        Task<MethodResult<bool>> UpdateMemberRoleAsync(Guid projectId, Guid memberId, string role, Guid? owner);


    }
}
