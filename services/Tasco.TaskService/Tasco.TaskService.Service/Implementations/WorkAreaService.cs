using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Repository.UnitOfWork;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Service.Interfaces;
using Tasco.Shared.Notifications.Models;


namespace Tasco.TaskService.Service.Implementations
{
    public class WorkAreaService : BaseService<WorkAreaService>, IWorkAreaService
    {
        private readonly IWorkTaskService _workTaskService;
        private readonly INotificationService _notificationService;
        public WorkAreaService(
            IUnitOfWork<TaskManagementDbContext> unitOfWork,
            ILogger<WorkAreaService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IWorkTaskService workTaskService,
            INotificationService notificationService
        ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _workTaskService = workTaskService;
            _notificationService = notificationService;
        }

        public async Task<WorkArea> CreateWorkArea(WorkAreaBusinessModel workArea)
        {
            var entity = _mapper.Map<WorkArea>(workArea);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;

            await _unitOfWork.GetRepository<WorkArea>().InsertAsync(entity);
            await _unitOfWork.CommitAsync();
            
            // Note: WorkArea creation notification is sent to project members through other channels
            // since WorkArea doesn't have direct task member relationships
            return entity;
        }

        public async Task DeleteWorkArea(Guid id)
        {
            var workArea = await _unitOfWork.GetRepository<WorkArea>()
                .SingleOrDefaultAsync(predicate: w => w.Id == id && !w.IsDeleted,
                include: q => q
                .Include(w => w.WorkTasks));

            if (workArea == null)
            {
                throw new KeyNotFoundException($"Work area with ID {id} not found.");
            }
            foreach (var task in workArea.WorkTasks)
            {
                await _workTaskService.DeleteWorkTask(task.Id);
            }
            _unitOfWork.GetRepository<WorkArea>().Delete(workArea);
            _logger.LogInformation($"Deleting work area with ID {id} and name {workArea.Name}.");
            await _unitOfWork.CommitAsync();
            // Send notification to all assigned members of all tasks in this work area
            var allMembers = new List<Tasco.TaskService.Repository.Entities.TaskMember>();
            foreach (var task in workArea.WorkTasks)
            {
                allMembers.AddRange(task.TaskMembers.Where(m => m.IsActive && !m.IsDeleted));
            }
            foreach (var member in allMembers.DistinctBy(m => m.UserId))
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = $"🗑️ Work Area Deleted: {workArea.Name}",
                    Message = $"Work area '{workArea.Name}' has been deleted.",
                    Type = NotificationType.TaskAssigned,
                    ProjectId = workArea.ProjectId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "workAreaName", workArea.Name },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
        }

        public async Task<IPaginate<WorkArea>> GetMyWorkAreasByProjectId(int pageSize, int pageIndex, Guid projectId)
        {
            var workArea = await _unitOfWork.GetRepository<WorkArea>().GetPagingListAsync(
                predicate: w => w.ProjectId == projectId && !w.IsDeleted,
                page: pageIndex,
                size: pageSize,
                include: query => query
                    .Include(w => w.WorkTasks)
                    .ThenInclude(wt => wt.TaskActions)
                    .Include(w => w.WorkTasks)
                    .ThenInclude(wt => wt.TaskMembers)
                    .Include(w => w.WorkTasks)
                    .ThenInclude(wt => wt.TaskObjectives)
            );

            return workArea;
        }


        public async Task<WorkArea> GetWorkAreaById(Guid id)
        {
            var workArea = await _unitOfWork.GetRepository<WorkArea>()
                .SingleOrDefaultAsync(
                    predicate: w => w.Id == id && !w.IsDeleted,
                    include: query => query
                        .Include(w => w.WorkTasks)
                        .ThenInclude(wt => wt.TaskActions)
                        .Include(w => w.WorkTasks)
                        .ThenInclude(wt => wt.TaskMembers)
                        .Include(w => w.WorkTasks)
                        .ThenInclude(wt => wt.TaskObjectives)
                    );

            if (workArea == null)
            {
                throw new KeyNotFoundException($"Work area with ID {id} not found.");
            }

            return workArea;
        }

        public async Task UpdateWorkArea(Guid id, WorkAreaBusinessModel workArea)
        {
            var existingWorkArea = await _unitOfWork.GetRepository<WorkArea>()
                .SingleOrDefaultAsync(predicate: w => w.Id == id && !w.IsDeleted);
            if (existingWorkArea == null)
            {
                throw new KeyNotFoundException($"Work area with ID {id} not found.");
            }

            _logger.LogInformation($"Updating work area with ID {id}");
            _logger.LogInformation($"Work area: {JsonConvert.SerializeObject(workArea)}");


            // Update the fields
            existingWorkArea.Name = workArea.Name;
            existingWorkArea.Description = workArea.Description;
            existingWorkArea.DisplayOrder = workArea.DisplayOrder;
            existingWorkArea.ProjectId = workArea.ProjectId;

            // Only update CreatedByUserId if it's not empty
            if (workArea.CreatedByUserId != Guid.Empty)
            {
                existingWorkArea.CreatedByUserId = workArea.CreatedByUserId;
            }

            _unitOfWork.GetRepository<WorkArea>().Update(existingWorkArea);
            await _unitOfWork.CommitAsync();
            // Send notification to all assigned members of all tasks in this work area
            var allMembers = new List<Tasco.TaskService.Repository.Entities.TaskMember>();
            foreach (var task in existingWorkArea.WorkTasks)
            {
                allMembers.AddRange(task.TaskMembers.Where(m => m.IsActive && !m.IsDeleted));
            }
            foreach (var member in allMembers.DistinctBy(m => m.UserId))
            {
                var message = new NotificationMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = member.UserId.ToString(),
                    Title = $"✏️ Work Area Updated: {existingWorkArea.Name}",
                    Message = $"Work area '{existingWorkArea.Name}' has been updated.",
                    Type = NotificationType.TaskAssigned,
                    ProjectId = existingWorkArea.ProjectId.ToString(),
                    Priority = NotificationPriority.Normal,
                    Channels = new List<NotificationChannel> { NotificationChannel.Email },
                    CreatedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "workAreaName", existingWorkArea.Name },
                        { "email", member.UserEmail ?? member.UserName }
                    }
                };
                await _notificationService.SendNotificationAsync(message);
            }
        }
    }
}