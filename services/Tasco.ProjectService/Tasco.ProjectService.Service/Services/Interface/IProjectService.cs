using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.ProjectService.Repository.Paginate;
using Tasco.ProjectService.Service.BussinessModel.ProjectBussinessModel;
using Tasco.ProjectService.Service.Helper;

namespace Tasco.ProjectService.Service.Services.Interface
{
    public interface IProjectService
    {
        Task<MethodResult<bool>> CreateProjectAsync(string name, string? description, Guid ownerId, string ownerName, string ownerEmail);
        Task<MethodResult<bool>> UpdateProjectAsync(Guid projectId, string? name, string? description, Guid updateBy);
        Task<MethodResult<bool>> DeleteProjectAsync(Guid projectId, Guid DeleteBy);
        Task<MethodResult<ProjectResponse>> GetProjectByIdAsync(Guid projectId);
        Task<MethodResult<IPaginate<ProjectResponse>>> GetProjectsAsync(int page = 1, int size = 10, string search = "", bool isdeleted = false);
        Task<MethodResult<IPaginate<ProjectResponse>>> GetProjectForMenber(Guid memberId, string? role, int page = 1, int size = 10, string search = "", bool isdeleted = false);


    }
}
