using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Repository.UnitOfWork;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Service.Interfaces;
using Tasco.Shared.Notifications.Models;

namespace Tasco.TaskService.Service.Implementations
{
    public class TaskObjectiveService : BaseService<TaskObjectiveService>, ITaskObjectiveService
    {
        private readonly INotificationService _notificationService;
        public TaskObjectiveService(
            IUnitOfWork<TaskManagementDbContext> unitOfWork,
            ILogger<TaskObjectiveService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService
        ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _notificationService = notificationService;
        }

        public async Task<TaskObjective> CreateTaskObjectiveAsync(TaskObjectiveBusinessModel taskObjective)
        {
            // Validate that the WorkTask exists and load task members
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == taskObjective.WorkTaskId && !wt.IsDeleted, 
                    include: t => t.Include(x => x.TaskMembers));
            
            if (workTask == null)
            {
                throw new ArgumentException($"WorkTask with ID {taskObjective.WorkTaskId} does not exist or has been deleted");
            }

            var entity = _mapper.Map<TaskObjective>(taskObjective);
            
            // Since authentication is disabled, use a default user ID
            var userId = Guid.NewGuid(); // Use a default user ID

            entity.Id = Guid.NewGuid();
            entity.CreatedByUserId = userId;
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsCompleted = false;
            entity.IsDeleted = false;

            await _unitOfWork.GetRepository<TaskObjective>().InsertAsync(entity);
            await _unitOfWork.CommitAsync();
            // Send notification to creator if they are also a task member
            var creatorMember = workTask.TaskMembers.FirstOrDefault(m => m.UserId == entity.CreatedByUserId && m.IsActive && !m.IsDeleted);
            if (creatorMember != null)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = entity.CreatedByUserId.ToString(),
                    Title = "🎯 Task Objective Created",
                    Message = $"Objective '{entity.Title}' created for task {entity.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = entity.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", entity.Title },
                        { "email", creatorMember.UserEmail ?? creatorMember.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
            // Send notification to all assigned members
            var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var notifyMessage = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = "🎯 Task Objective Created",
                    Message = $"Objective '{entity.Title}' created for task {entity.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = entity.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", entity.Title },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(notifyMessage);
            }
            return entity;
        }

        public async Task<TaskObjective> GetTaskObjectiveByIdAsync(Guid id)
        {
            var taskObjective = await _unitOfWork.GetRepository<TaskObjective>()
                .SingleOrDefaultAsync(
                    predicate: to => to.Id == id && !to.IsDeleted
                );

            if (taskObjective == null)
            {
                throw new KeyNotFoundException($"Task objective with ID {id} not found.");
            }

            return taskObjective;
        }

        public async Task<IPaginate<TaskObjective>> GetTaskObjectivesByWorkTaskIdAsync(Guid workTaskId, int pageIndex, int pageSize)
        {
            return await _unitOfWork.GetRepository<TaskObjective>()
                .GetPagingListAsync(
                    predicate: to => to.WorkTaskId == workTaskId && !to.IsDeleted,
                    orderBy: q => q.OrderBy(to => to.DisplayOrder),
                    page: pageIndex,
                    size: pageSize 
                );
        }

        public async Task<TaskObjective> UpdateTaskObjectiveAsync(Guid id, TaskObjectiveBusinessModel taskObjective)
        {
            var existingTaskObjective = await _unitOfWork.GetRepository<TaskObjective>()
                .SingleOrDefaultAsync(predicate: to => to.Id == id && !to.IsDeleted);

            if (existingTaskObjective == null)
            {
                throw new KeyNotFoundException($"Task objective with ID {id} not found.");
            }

            // Validate that the WorkTask exists and load task members
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == taskObjective.WorkTaskId && !wt.IsDeleted, 
                    include: t => t.Include(x => x.TaskMembers));
            
            if (workTask == null)
            {
                throw new ArgumentException($"WorkTask with ID {taskObjective.WorkTaskId} does not exist or has been deleted");
            }

            // Preserve original creation data
            var createdByUserId = existingTaskObjective.CreatedByUserId;
            var createdDate = existingTaskObjective.CreatedDate;

            // Map new data
            _mapper.Map(taskObjective, existingTaskObjective);

            // Restore original creation data
            existingTaskObjective.CreatedByUserId = createdByUserId;
            existingTaskObjective.CreatedDate = createdDate;

            _unitOfWork.GetRepository<TaskObjective>().Update(existingTaskObjective);
            await _unitOfWork.CommitAsync();
            // Send notification to creator if they are also a task member
            var creatorMember = workTask.TaskMembers.FirstOrDefault(m => m.UserId == existingTaskObjective.CreatedByUserId && m.IsActive && !m.IsDeleted);
            if (creatorMember != null)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = existingTaskObjective.CreatedByUserId.ToString(),
                    Title = "✏️ Task Objective Updated",
                    Message = $"Objective '{existingTaskObjective.Title}' updated for task {existingTaskObjective.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = existingTaskObjective.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", existingTaskObjective.Title },
                        { "email", creatorMember.UserEmail ?? creatorMember.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
            // Send notification to all assigned members
            var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var notifyMessage = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = "✏️ Task Objective Updated",
                    Message = $"Objective '{existingTaskObjective.Title}' updated for task {existingTaskObjective.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = existingTaskObjective.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", existingTaskObjective.Title },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(notifyMessage);
            }
            return existingTaskObjective;
        }

        public async Task DeleteTaskObjectiveAsync(Guid id)
        {
            var taskObjective = await _unitOfWork.GetRepository<TaskObjective>()
                .SingleOrDefaultAsync(predicate: to => to.Id == id && !to.IsDeleted);

            if (taskObjective == null)
            {
                throw new KeyNotFoundException($"Task objective with ID {id} not found.");
            }

            taskObjective.IsDeleted = true;
            _unitOfWork.GetRepository<TaskObjective>().Update(taskObjective);
            await _unitOfWork.CommitAsync();
            // Get work task with members for notifications
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == taskObjective.WorkTaskId && !wt.IsDeleted, include: t => t.Include(x => x.TaskMembers));
            
            // Send notification to creator if they are also a task member
            var creatorMember = workTask.TaskMembers.FirstOrDefault(m => m.UserId == taskObjective.CreatedByUserId && m.IsActive && !m.IsDeleted);
            if (creatorMember != null)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = taskObjective.CreatedByUserId.ToString(),
                    Title = "🗑️ Task Objective Deleted",
                    Message = $"Objective '{taskObjective.Title}' deleted from task {taskObjective.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = taskObjective.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", taskObjective.Title },
                        { "email", creatorMember.UserEmail ?? creatorMember.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
            // Send notification to all assigned members
            var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var notifyMessage = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = "🗑️ Task Objective Deleted",
                    Message = $"Objective '{taskObjective.Title}' deleted from task {taskObjective.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = taskObjective.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", taskObjective.Title },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(notifyMessage);
            }
        }

        public async Task<TaskObjective> CompleteTaskObjectiveAsync(Guid id, bool isCompleted, Guid userId)
        {
            var taskObjective = await _unitOfWork.GetRepository<TaskObjective>()
                .SingleOrDefaultAsync(predicate: to => to.Id == id && !to.IsDeleted);

            if (taskObjective == null)
            {
                throw new KeyNotFoundException($"Task objective with ID {id} not found.");
            }

            taskObjective.IsCompleted = isCompleted;
            taskObjective.CompletedDate = isCompleted ? DateTime.UtcNow : null;
            taskObjective.CompletedByUserId = isCompleted ? userId : null;

            _unitOfWork.GetRepository<TaskObjective>().Update(taskObjective);
            await _unitOfWork.CommitAsync();
            // Get work task with members for notifications
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == taskObjective.WorkTaskId && !wt.IsDeleted, include: t => t.Include(x => x.TaskMembers));
            
            // Send notification to creator if they are also a task member
            var creatorMember = workTask.TaskMembers.FirstOrDefault(m => m.UserId == taskObjective.CreatedByUserId && m.IsActive && !m.IsDeleted);
            if (creatorMember != null)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = taskObjective.CreatedByUserId.ToString(),
                    Title = isCompleted ? "✅ Task Objective Completed" : "❌ Task Objective Marked Incomplete",
                    Message = $"Objective '{taskObjective.Title}' {(isCompleted ? "completed" : "marked incomplete")} for task {taskObjective.WorkTaskId}.",
                    Type = isCompleted ? NotificationType.TaskStatusChanged : NotificationType.TaskAssigned,
                    TaskId = taskObjective.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", taskObjective.Title },
                        { "email", creatorMember.UserEmail ?? creatorMember.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
            // Send notification to all assigned members
            var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var notifyMessage = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = isCompleted ? "✅ Task Objective Completed" : "❌ Task Objective Marked Incomplete",
                    Message = $"Objective '{taskObjective.Title}' {(isCompleted ? "completed" : "marked incomplete")} for task {taskObjective.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = taskObjective.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "objectiveTitle", taskObjective.Title },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(notifyMessage);
            }
            return taskObjective;
        }
    }
}