using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.BusinessModels;

namespace Tasco.TaskService.Service.Interfaces
{
    public interface IWorkAreaService
    {
        Task<IPaginate<WorkArea>>  GetMyWorkAreasByProjectId(int pageSize, int pageIndex,Guid projectId);
        Task<WorkArea> GetWorkAreaById(Guid id);
        Task<WorkArea> CreateWorkArea(WorkAreaBusinessModel workArea);
        Task UpdateWorkArea(Guid id, WorkAreaBusinessModel workArea);
        Task DeleteWorkArea(Guid id);
    }
}
