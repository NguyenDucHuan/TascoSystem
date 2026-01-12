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
    public interface ITaskMemberService
    {
        Task AssignWorkTaskToUser(Guid workTaskId, Guid userId);
        Task<TaskMember> CreateTaskMember(TaskMemberBusinessModel taskMember);
        Task<TaskMember> GetTaskMember(Guid taskMemberId);
        Task<TaskMember> UpdateTaskMember(Guid taskMemberId, TaskMemberBusinessModel taskMember);
        Task<TaskMember> DeleteTaskMember(Guid taskMemberId);
        
        // Additional methods needed by gRPC service
        Task<TaskMember> GetTaskMemberById(Guid taskMemberId);
        Task<IPaginate<TaskMember>> GetTaskMembersByTaskId(int pageSize, int pageIndex, Guid workTaskId);
        Task<TaskMember> RemoveTaskMember(Guid taskMemberId, Guid removedByUserId);
    }
}
