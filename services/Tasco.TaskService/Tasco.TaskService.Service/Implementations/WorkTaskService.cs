using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Repository.UnitOfWork;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Service.Interfaces;
using Tasco.Shared.Notifications.Models;

namespace Tasco.TaskService.Service.Implementations
{
    public class WorkTaskService : BaseService<WorkTaskService>, IWorkTaskService
    {
        private readonly ITaskActionService _taskActionService;
        private readonly INotificationService _notificationService;

        public WorkTaskService(
            IUnitOfWork<TaskManagementDbContext> unitOfWork,
            ILogger<WorkTaskService> logger, IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ITaskActionService taskActionService,
            INotificationService notificationService
        ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _taskActionService = taskActionService;
            _notificationService = notificationService;
        }

        public async Task<WorkTask> CreateWorkTask(WorkTaskBusinessModel workTask)
        {
            var entity = _mapper.Map<WorkTask>(workTask);


            // Set creation info
            entity.CreatedByUserId = workTask.CreatedByUserId;
            entity.CreatedByUserName = workTask.CreatedByUserName;
            entity.WorkAreaId = workTask.WorkAreaId;
            entity.CreatedDate = DateTime.UtcNow;
            entity.Progress = 0;

            await _unitOfWork.GetRepository<WorkTask>().InsertAsync(entity);
            await _unitOfWork.CommitAsync();

            // Log task creation
            await _taskActionService.CreateTaskAction(new TaskActionBusinessModel
            {
                WorkTaskId = entity.Id,
                ActionType = "Created",
                Description = $"Task '{entity.Title}' created",
                ActionDate = DateTime.UtcNow
            });

            // Send notification to creator (who is automatically added as a task member)
            var creatorMember = entity.TaskMembers.FirstOrDefault(m => m.UserId == entity.CreatedByUserId);
            if (creatorMember != null)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = entity.CreatedByUserId.ToString(),
                    Title = $"📝 Task Created: {entity.Title}",
                    Message = $"Task '{entity.Title}' has been created.",
                    Type = NotificationType.TaskAssigned,
                    ProjectId = entity.WorkAreaId.ToString(),
                    TaskId = entity.Id.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "taskTitle", entity.Title },
                        { "createdBy", entity.CreatedByUserName },
                        { "email", creatorMember.UserEmail ?? creatorMember.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }

            return entity;
        }

        public async Task DeleteWorkTask(Guid id)
        {
            var task = await _unitOfWork.GetRepository<WorkTask>()
            .SingleOrDefaultAsync(
                predicate: t => t.Id == id,
                include: t => t
                    .Include(x => x.TaskMembers)
                    .Include(x => x.TaskObjectives)
                    .Include(x => x.TaskActions)
                    .Include(x => x.Comments)
            );

            if (task == null)
            {
                throw new KeyNotFoundException($"Work task with ID {id} not found.");
            }
            _unitOfWork.GetRepository<TaskMember>().DeleteRange(task.TaskMembers);
            _unitOfWork.GetRepository<TaskObjective>().DeleteRange(task.TaskObjectives);
            _unitOfWork.GetRepository<TaskAction>().DeleteRange(task.TaskActions);
            _unitOfWork.GetRepository<Comment>().DeleteRange(task.Comments);

            _unitOfWork.GetRepository<WorkTask>().Delete(task);
            await _unitOfWork.CommitAsync();

            // Send notification to all assigned members
            var assignedMembers = task.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = $"🗑️ Task Deleted: {task.Title}",
                    Message = $"Task '{task.Title}' has been deleted.",
                    Type = NotificationType.TaskAssigned,
                    ProjectId = task.WorkAreaId.ToString(),
                    TaskId = task.Id.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "taskTitle", task.Title },
                        { "deletedBy", task.CreatedByUserName },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
        }

        public async Task<IPaginate<WorkTask>> GetAllWorkTasks(int pageSize, int pageIndex, string search = null)
        {
            var workTasks = await _unitOfWork.GetRepository<WorkTask>().GetPagingListAsync
            (predicate: t => !t.IsDeleted && (string.IsNullOrEmpty(search) ||
                                              t.Title.Contains(search) ||
                                              t.Description.Contains(search)),
                orderBy: q => q.OrderByDescending(t => t.CreatedDate),
                page: pageIndex,
                size: pageSize);
            return workTasks;
        }

        public async Task<IPaginate<WorkTask>> GetMyWorkTasks(int pageSize, int pageIndex, string search = null)
        {
            // Since authentication is disabled, return all tasks or implement alternative logic
            var query = await _unitOfWork.GetRepository<WorkTask>().GetPagingListAsync(
                predicate: t => !t.IsDeleted,
                orderBy: q => q.OrderByDescending(t => t.CreatedDate),
                page: pageIndex,
                size: pageSize);

            return query;
        }

        public async Task<WorkTask> GetWorkTaskById(Guid id)
        {
            var task = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(
                    predicate: t => t.Id == id && !t.IsDeleted,
                    include: t => t.Include(x => x.TaskMembers)
                        .Include(x => x.TaskObjectives)
                        .Include(x => x.TaskActions)
                        .Include(x => x.WorkArea));

            if (task == null)
            {
                throw new KeyNotFoundException($"Work task with ID {id} not found.");
            }

            return task;
        }

        public async Task UpdateWorkTask(Guid id, WorkTaskBusinessModel workTask)
        {
            if (workTask == null)
            {
                throw new ArgumentNullException(nameof(workTask), "WorkTask data cannot be null");
            }

            var existingTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(
                    predicate: t => t.Id == id,
                    include: t => t.Include(x => x.TaskMembers)
                        .Include(x => x.TaskObjectives));

            if (existingTask == null)
            {
                throw new KeyNotFoundException($"Work task with ID {id} not found.");
            }

            // Use user information from the request instead of JWT
            var userId = workTask.CreatedByUserId.ToString();
            var userEmail = workTask.CreatedByUserName;

            // Track changes for logging
            var changes = new List<string>();
            var oldValues = new List<string>();
            var newValues = new List<string>();

            if (existingTask.Title != workTask.Title)
            {
                changes.Add("Title");
                oldValues.Add(existingTask.Title ?? "");
                newValues.Add(workTask.Title ?? "");
            }

            if (existingTask.Description != workTask.Description)
            {
                changes.Add("Description");
                oldValues.Add(existingTask.Description ?? "");
                newValues.Add(workTask.Description ?? "");
            }

            if (existingTask.Status != workTask.Status)
            {
                changes.Add("Status");
                oldValues.Add(existingTask.Status ?? "");
                newValues.Add(workTask.Status ?? "");
            }

            if (existingTask.Priority != workTask.Priority)
            {
                changes.Add("Priority");
                oldValues.Add(existingTask.Priority ?? "");
                newValues.Add(workTask.Priority ?? "");
            }

            if (existingTask.StartDate != workTask.StartDate)
            {
                changes.Add("StartDate");
                oldValues.Add(existingTask.StartDate?.ToString("yyyy-MM-dd") ?? "");
                newValues.Add(workTask.StartDate?.ToString("yyyy-MM-dd") ?? "");
            }

            if (existingTask.EndDate != workTask.EndDate)
            {
                changes.Add("EndDate");
                oldValues.Add(existingTask.EndDate?.ToString("yyyy-MM-dd") ?? "");
                newValues.Add(workTask.EndDate?.ToString("yyyy-MM-dd") ?? "");
            }

            if (existingTask.DueDate != workTask.DueDate)
            {
                changes.Add("DueDate");
                oldValues.Add(existingTask.DueDate?.ToString("yyyy-MM-dd") ?? "");
                newValues.Add(workTask.DueDate?.ToString("yyyy-MM-dd") ?? "");
            }

            if (existingTask.Progress != workTask.Progress)
            {
                changes.Add("Progress");
                oldValues.Add(existingTask.Progress.ToString());
                newValues.Add(workTask.Progress.ToString());
            }
            if (existingTask.DisplayOrder != workTask.DisplayOrder)
            {
                changes.Add("DisplayOrder");
                oldValues.Add(existingTask.DisplayOrder.ToString());
                newValues.Add(workTask.DisplayOrder.ToString());
            }

            if (existingTask.WorkAreaId != workTask.WorkAreaId)
            {
                changes.Add("WorkAreaId");
                oldValues.Add(existingTask.WorkAreaId.ToString());
                newValues.Add(workTask.WorkAreaId.ToString());
            }

            // Preserve original creation data
            var createdByUserId = existingTask.CreatedByUserId;
            var createdByUserName = existingTask.CreatedByUserName;
            var createdDate = existingTask.CreatedDate;

            // Map new data
            _mapper.Map(workTask, existingTask);

            // Restore original creation data
            existingTask.CreatedByUserId = createdByUserId;
            existingTask.CreatedByUserName = createdByUserName;
            existingTask.CreatedDate = createdDate;

            // Update the task
            _unitOfWork.GetRepository<WorkTask>().Update(existingTask);
            await _unitOfWork.CommitAsync();

            // Log changes if any
            if (changes.Any())
            {
                for (int i = 0; i < changes.Count; i++)
                {
                    await _taskActionService.CreateTaskAction(new TaskActionBusinessModel
                    {
                        WorkTaskId = id,
                        ActionType = "Updated",
                        Description = $"Task '{existingTask.Title}' {changes[i].ToLower()} updated",
                        OldValue = oldValues[i],
                        NewValue = newValues[i],
                        ActionDate = DateTime.UtcNow
                    });
                }
                // Send notification for update to all assigned members
                var assignedMembers = existingTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
                foreach (var member in assignedMembers)
                {
                    var message = new NotificationMessage
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = member.UserId.ToString(),
                        Title = $"✏️ Task Updated: {existingTask.Title}",
                        Message = $"Task '{existingTask.Title}' has been updated.",
                        Type = NotificationType.TaskAssigned,
                        ProjectId = existingTask.WorkAreaId.ToString(),
                        TaskId = existingTask.Id.ToString(),
                        Priority = NotificationPriority.Normal,
                        Channels = new List<NotificationChannel> { NotificationChannel.Email },
                        CreatedAt = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                        {
                            { "taskTitle", existingTask.Title },
                            { "updatedBy", existingTask.CreatedByUserName },
                            { "email", member.UserEmail ?? member.UserName },
                            { "changes", string.Join(", ", changes) }
                        }
                    };
                    await _notificationService.SendNotificationAsync(message);
                }
            }
        }
    }
}