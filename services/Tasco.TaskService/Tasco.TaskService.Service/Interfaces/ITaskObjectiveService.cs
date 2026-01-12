using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.BusinessModels;

namespace Tasco.TaskService.Service.Interfaces
{
    public interface ITaskObjectiveService
    {
        Task<TaskObjective> CreateTaskObjectiveAsync(TaskObjectiveBusinessModel taskObjective);
        Task<TaskObjective> GetTaskObjectiveByIdAsync(Guid id);
        Task<IPaginate<TaskObjective>> GetTaskObjectivesByWorkTaskIdAsync(Guid workTaskId, int pageIndex, int pageSize);
        Task<TaskObjective> UpdateTaskObjectiveAsync(Guid id, TaskObjectiveBusinessModel taskObjective);
        Task DeleteTaskObjectiveAsync(Guid id);
        Task<TaskObjective> CompleteTaskObjectiveAsync(Guid id, bool isCompleted, Guid userId);
    }
} 