using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.BusinessModels;

namespace Tasco.TaskService.Service.Interfaces
{
    public interface IWorkTaskService
    {
        Task<WorkTask> GetWorkTaskById(Guid id);
        Task<IPaginate<WorkTask>> GetAllWorkTasks(int pageSize, int pageIndex, string search = null);
        Task<IPaginate<WorkTask>> GetMyWorkTasks(int pageSize, int pageIndex, string search = null);
        Task<WorkTask> CreateWorkTask(WorkTaskBusinessModel workTask);
        Task UpdateWorkTask(Guid id, WorkTaskBusinessModel workTask);
        Task DeleteWorkTask(Guid id);
    }
}
