using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.BusinessModels;

namespace Tasco.TaskService.Service.Interfaces;

public interface ICommentService
{
    Task<Comment> AddCommentAsync(Guid taskId, CommentBusinessModel request);
    Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(Guid taskId);
    Task<IPaginate<Comment>> GetCommentsByTaskIdWithPaginationAsync(Guid taskId, int pageSize, int pageIndex);
    Task<Comment> UpdateCommentAsync(Guid commentId, CommentBusinessModel request, Guid userId);
    Task DeleteCommentAsync(Guid commentId, Guid userId);
}