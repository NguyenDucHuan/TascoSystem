using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.UnitOfWork;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Service.Interfaces;
using Tasco.Shared.Notifications.Models;
using Microsoft.EntityFrameworkCore;

namespace Tasco.TaskService.Service.Implementations
    {
    public class TaskMemberService : BaseService<TaskMemberService>, ITaskMemberService
    {
        private readonly INotificationService _notificationService;
        public TaskMemberService(IUnitOfWork<TaskManagementDbContext> unitOfWork, ILogger<TaskMemberService> logger,
            IMapper mapper, IHttpContextAccessor httpContextAccessor, INotificationService notificationService) : base(unitOfWork, logger, mapper,
            httpContextAccessor)
        {
            _notificationService = notificationService;
        }

        public async Task AssignWorkTaskToUser(Guid workTaskId, Guid userId, string userName)
        {
            // Validate that the WorkTask exists
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == workTaskId && !wt.IsDeleted);
            
            if (workTask == null)
            {
                throw new ArgumentException($"WorkTask with ID {workTaskId} does not exist or has been deleted");
            }

            // Check if user is already assigned to this task
            var existingMember = await _unitOfWork.GetRepository<TaskMember>()
                .SingleOrDefaultAsync(m => m.WorkTaskId == workTaskId && m.UserId == userId && !m.IsDeleted);

            if (existingMember != null)
            {
                throw new InvalidOperationException($"User {userId} is already assigned to WorkTask {workTaskId}");
            }

            var taskMember = new TaskMember
            {
                WorkTaskId = workTaskId,
                UserId = userId,
                UserName = userName,
                Role = "Assignee",
                AssignedByUserId = userId,
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };
            await _unitOfWork.GetRepository<TaskMember>().InsertAsync(taskMember);
            await _unitOfWork.CommitAsync();
            // Send notification to all assigned members
            var workTaskNoti = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == workTaskId && !wt.IsDeleted, include: t => t.Include(x => x.TaskMembers));
            var assignedMembers = workTaskNoti.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = "👤 Task Assigned",
                    Message = $"User {userName} assigned to task {workTaskId}.",
                    Type = NotificationType.TaskAssigned,
                    TaskId = workTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "userName", userName },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
        }

        public async Task RemoveWorkTaskFromUser(Guid workTaskId, Guid userId)
        {
            var member = await _unitOfWork.GetRepository<TaskMember>()
                .SingleOrDefaultAsync(m => m.WorkTaskId == workTaskId && m.UserId == userId && !m.IsDeleted);

            if (member != null)
            {
                member.IsActive = false;
                member.IsDeleted = true;
                _unitOfWork.GetRepository<TaskMember>().Update(member);
                await _unitOfWork.CommitAsync();
            }
        }

        public Task AssignWorkTaskToUser(Guid workTaskId, Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<TaskMember> CreateTaskMember(TaskMemberBusinessModel taskMember)
        {
            // Validate that the WorkTask exists
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == taskMember.WorkTaskId && !wt.IsDeleted);
            
            if (workTask == null)
            {
                throw new ArgumentException($"WorkTask with ID {taskMember.WorkTaskId} does not exist or has been deleted");
            }

            // Check if user is already assigned to this task
            var existingMember = await _unitOfWork.GetRepository<TaskMember>()
                .SingleOrDefaultAsync(m => m.WorkTaskId == taskMember.WorkTaskId && m.UserId == taskMember.UserId && !m.IsDeleted);

            if (existingMember != null)
            {
                throw new InvalidOperationException($"User {taskMember.UserId} is already assigned to WorkTask {taskMember.WorkTaskId}");
            }

            var task = _mapper.Map<TaskMember>(taskMember);
            await _unitOfWork.GetRepository<TaskMember>().InsertAsync(task);
            await _unitOfWork.CommitAsync();
            // Send notification to all assigned members
            var workTaskNoti = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == task.WorkTaskId && !wt.IsDeleted, include: t => t.Include(x => x.TaskMembers));
            var assignedMembers = workTaskNoti.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = "👤 Task Member Created",
                    Message = $"Task member {task.UserName} created for task {task.WorkTaskId}.",
                    Type = NotificationType.TaskAssigned,
                    TaskId = task.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "userName", task.UserName },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
            return task;
        }

        public async Task<TaskMember> GetTaskMember(Guid taskMemberId)
        {
            return await _unitOfWork.GetRepository<TaskMember>().SingleOrDefaultAsync(predicate: x => x.Id == taskMemberId && !x.IsDeleted);
        }

        public async Task<TaskMember> UpdateTaskMember(Guid taskMemberId, TaskMemberBusinessModel taskMember)
        {
            var existingTaskMember = await _unitOfWork.GetRepository<TaskMember>().SingleOrDefaultAsync(predicate: x => x.Id == taskMemberId && !x.IsDeleted);
            if (existingTaskMember == null)
            {
                throw new ArgumentException($"Task member with ID {taskMemberId} not found");
            }

            // Only update specific fields, preserve others
            existingTaskMember.Role = taskMember.Role;
            existingTaskMember.IsActive = taskMember.IsActive;
            existingTaskMember.AssignedByUserId = taskMember.AssignedByUserId;
            // Don't update WorkTaskId, UserId, UserName, UserEmail, AssignedDate as these shouldn't change
            
            _unitOfWork.GetRepository<TaskMember>().Update(existingTaskMember);
            await _unitOfWork.CommitAsync();
            // Send notification to all assigned members
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == existingTaskMember.WorkTaskId && !wt.IsDeleted, include: t => t.Include(x => x.TaskMembers));
            var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = "✏️ Task Member Updated",
                    Message = $"Task member {existingTaskMember.UserName} updated for task {existingTaskMember.WorkTaskId}.",
                    Type = NotificationType.TaskAssigned,
                    TaskId = existingTaskMember.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "userName", existingTaskMember.UserName },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
            return existingTaskMember;
        }

        public async Task<TaskMember> DeleteTaskMember(Guid taskMemberId)
        {
            var taskMember = await _unitOfWork.GetRepository<TaskMember>().SingleOrDefaultAsync(predicate: x => x.Id == taskMemberId);
            _unitOfWork.GetRepository<TaskMember>().Delete(taskMember);
            await _unitOfWork.CommitAsync();
            // Send notification to all assigned members
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == taskMember.WorkTaskId && !wt.IsDeleted, include: t => t.Include(x => x.TaskMembers));
            var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = "🗑️ Task Member Deleted",
                    Message = $"Task member {taskMember.UserName} deleted from task {taskMember.WorkTaskId}.",
                    Type = NotificationType.TaskAssigned,
                    TaskId = taskMember.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "userName", taskMember.UserName },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
            return taskMember;
        }

        // Additional methods needed by gRPC service
        public async Task<TaskMember> GetTaskMemberById(Guid taskMemberId)
        {
            return await _unitOfWork.GetRepository<TaskMember>().SingleOrDefaultAsync(predicate: x => x.Id == taskMemberId && !x.IsDeleted);
        }

        public async Task<IPaginate<TaskMember>> GetTaskMembersByTaskId(int pageSize, int pageIndex, Guid workTaskId)
        {
            var taskMembers = await _unitOfWork.GetRepository<TaskMember>().GetPagingListAsync(
                predicate: x => x.WorkTaskId == workTaskId && !x.IsDeleted,
                page: pageIndex ,
                size: pageSize
            );
            return taskMembers;
        }

        public async Task<TaskMember> RemoveTaskMember(Guid taskMemberId, Guid removedByUserId)
        {
            var taskMember = await _unitOfWork.GetRepository<TaskMember>().SingleOrDefaultAsync(predicate: x => x.Id == taskMemberId && !x.IsDeleted);
            if (taskMember == null)
            {
                throw new ArgumentException($"Task member with ID {taskMemberId} not found");
            }

            taskMember.IsActive = false;
            taskMember.IsDeleted = true;
            // You might want to add additional fields like RemovedByUserId, RemovedDate, etc.
            
            _unitOfWork.GetRepository<TaskMember>().Update(taskMember);
            await _unitOfWork.CommitAsync();
            return taskMember;
        }
    }
}