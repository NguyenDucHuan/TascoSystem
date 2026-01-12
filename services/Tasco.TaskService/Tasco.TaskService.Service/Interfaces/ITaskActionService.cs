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
    public interface ITaskActionService
    {
        Task CreateTaskAction(TaskActionBusinessModel taskAction);
        Task<IPaginate<TaskAction>> GetTaskActionsByTaskId(Guid taskId, int pageSize = 10, int pageIndex = 1);
        Task<TaskAction> GetTaskActionById(Guid id);

    }
}
