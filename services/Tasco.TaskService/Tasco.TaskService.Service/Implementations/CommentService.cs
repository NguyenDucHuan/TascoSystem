using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.UnitOfWork;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Service.Interfaces;
using Tasco.Shared.Notifications.Models;

namespace Tasco.TaskService.Service.Implementations;

public class CommentService : BaseService<CommentService>, ICommentService
{
    private readonly INotificationService _notificationService;
    private readonly IWorkTaskService _workTaskService;
    public CommentService(
        IUnitOfWork<TaskManagementDbContext> unitOfWork,
        ILogger<CommentService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        INotificationService notificationService,
        IWorkTaskService workTaskService
    ) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _notificationService = notificationService;
        _workTaskService = workTaskService;
    }

    public async Task<Comment> AddCommentAsync(Guid taskId, CommentBusinessModel request)
    {
        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = request.UserId,
            UserName = request.UserName,
            Content = request.Content,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
        await _unitOfWork.GetRepository<Comment>().InsertAsync(comment);
        await _unitOfWork.CommitAsync();
        // Send notification to all assigned users
        var workTask = await _workTaskService.GetWorkTaskById(taskId);
        var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
        foreach (var member in assignedMembers)
        {
            var message = new NotificationMessage
            {
                Id = Guid.NewGuid().ToString(),
                UserId = member.UserId.ToString(),
                Title = "💬 New Comment Added",
                Message = $"A new comment was added to task {taskId} by {request.UserName}.",
                Type = NotificationType.TaskCommentAdded,
                TaskId = taskId.ToString(),
                Priority = NotificationPriority.Normal,
                Channels = new List<NotificationChannel> { NotificationChannel.Email },
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "comment", request.Content },
                    { "author", request.UserName },
                    { "email", member.UserEmail ?? member.UserName }
                }
            };
            await _notificationService.SendNotificationAsync(message);
        }
        return comment;
    }

    public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(Guid taskId)
    {
        return await _unitOfWork.GetRepository<Comment>()
            .GetListAsync(predicate: c => c.TaskId == taskId && !c.IsDeleted,
                orderBy: q => q.OrderByDescending(c => c.CreatedAt));
    }

    public async Task<IPaginate<Comment>> GetCommentsByTaskIdWithPaginationAsync(Guid taskId, int pageSize, int pageIndex)
    {
        return await _unitOfWork.GetRepository<Comment>().GetPagingListAsync(
            predicate: c => c.TaskId == taskId && !c.IsDeleted,
            orderBy: q => q.OrderByDescending(c => c.CreatedAt),
            page: pageIndex, // Repository already uses 1-based indexing
            size: pageSize
        );
    }

    public async Task<Comment> UpdateCommentAsync(Guid commentId, CommentBusinessModel request, Guid userId)
    {
        var comment = await _unitOfWork.GetRepository<Comment>()
            .SingleOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} not found.");
        }

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own comments.");
        }

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.GetRepository<Comment>().Update(comment);
        await _unitOfWork.CommitAsync();
        // Send notification to all assigned users
        var workTask = await _workTaskService.GetWorkTaskById(comment.TaskId);
        var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
        foreach (var member in assignedMembers)
        {
            var message = new NotificationMessage
            {
                Id = Guid.NewGuid().ToString(),
                UserId = member.UserId.ToString(),
                Title = "✏️ Comment Updated",
                Message = $"A comment was updated on task {comment.TaskId} by {request.UserName}.",
                Type = NotificationType.TaskCommentAdded,
                TaskId = comment.TaskId.ToString(),
                Priority = NotificationPriority.Normal,
                Channels = new List<NotificationChannel> { NotificationChannel.Email },
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "comment", request.Content },
                    { "author", request.UserName },
                    { "email", member.UserEmail ?? member.UserName }
                }
            };
            await _notificationService.SendNotificationAsync(message);
        }
        return comment;
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _unitOfWork.GetRepository<Comment>()
            .SingleOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (comment == null)
        {
            throw new KeyNotFoundException($"Comment with ID {commentId} not found.");
        }

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own comments.");
        }

        comment.IsDeleted = true;
        _unitOfWork.GetRepository<Comment>().Update(comment);
        await _unitOfWork.CommitAsync();
        // Send notification to all assigned users
        var workTask = await _workTaskService.GetWorkTaskById(comment.TaskId);
        var assignedMembers = workTask.TaskMembers.Where(m => m.IsActive && !m.IsDeleted).ToList();
        foreach (var member in assignedMembers)
        {
            var message = new NotificationMessage
            {
                Id = Guid.NewGuid().ToString(),
                UserId = member.UserId.ToString(),
                Title = "🗑️ Comment Deleted",
                Message = $"A comment was deleted on task {comment.TaskId} by user.",
                Type = NotificationType.TaskStatusChanged,
                TaskId = comment.TaskId.ToString(),
                Priority = NotificationPriority.Normal,
                Channels = new List<NotificationChannel> { NotificationChannel.Email },
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "commentId", comment.Id.ToString() },
                    { "email", member.UserEmail ?? member.UserName }
                }
            };
            await _notificationService.SendNotificationAsync(message);
        }
    }
}