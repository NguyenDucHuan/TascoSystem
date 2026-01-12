using AutoMapper;
using Microsoft.AspNetCore.Http;
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
using Microsoft.EntityFrameworkCore;

namespace Tasco.TaskService.Service.Implementations
{
    public class TaskActionService : BaseService<TaskActionService>, ITaskActionService
    {
        private readonly INotificationService _notificationService;
        public TaskActionService(
            IUnitOfWork<TaskManagementDbContext> unitOfWork,
            ILogger<TaskActionService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService
        ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _notificationService = notificationService;
        }

        public async Task CreateTaskAction(TaskActionBusinessModel taskAction)
        {
            var entity = _mapper.Map<TaskAction>(taskAction);
            
            // Use user information from the business model instead of JWT
            // Since authentication is disabled, we'll use default values or values from the model
            if (entity.UserId == Guid.Empty)
            {
                entity.UserId = Guid.NewGuid(); // Use a default user ID
            }
            if (string.IsNullOrEmpty(entity.UserName))
            {
                entity.UserName = "System User"; // Use a default user name
            }
            
            await _unitOfWork.GetRepository<TaskAction>().InsertAsync(entity);
            await _unitOfWork.CommitAsync();
            // Send notification to all assigned members
            var workTask = await _unitOfWork.GetRepository<WorkTask>()
                .SingleOrDefaultAsync(wt => wt.Id == entity.WorkTaskId && !wt.IsDeleted, include: t => t.Include(x => x.TaskMembers));
            var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
            foreach (var member in assignedMembers)
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = $"🔔 Task Action: {entity.ActionType}",
                    Message = $"Action '{entity.ActionType}' performed on task {entity.WorkTaskId}.",
                    Type = NotificationType.TaskStatusChanged,
                    TaskId = entity.WorkTaskId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "actionType", entity.ActionType },
                        { "description", entity.Description },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
        }

        public async Task<TaskAction> GetTaskActionById(Guid id)
        {
            var taskAction = await _unitOfWork.GetRepository<TaskAction>()
                .SingleOrDefaultAsync(predicate: t => t.Id == id);

            if (taskAction == null)
            {
                throw new KeyNotFoundException($"Task action with ID {id} not found.");
            }

            return taskAction;
        }

        public async Task<IPaginate<TaskAction>> GetTaskActionsByTaskId(Guid taskId, int pageSize = 10,
            int pageIndex = 1)
        {
            var taskActions = await _unitOfWork.GetRepository<TaskAction>().GetPagingListAsync(
                predicate: t => t.WorkTaskId == taskId,
                orderBy: q => q.OrderByDescending(t => t.ActionDate),
                page: pageIndex,
                size: pageSize
            );
            return taskActions;
        }
    }
}