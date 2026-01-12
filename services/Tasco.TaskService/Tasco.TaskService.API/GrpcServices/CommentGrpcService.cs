using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Service.Interfaces;
using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Repository.Paginate;
using Tasco.TaskService.Service.Implementations;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace Tasco.TaskService.API.GrpcServices
{
    public class CommentGrpcService : Protos.CommentService.CommentServiceBase
    {
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentGrpcService> _logger;

        public CommentGrpcService(
            ICommentService commentService,
            IMapper mapper,
            ILogger<CommentGrpcService> logger)
        {
            _commentService = commentService;
            _mapper = mapper;
            _logger = logger;
        }

        private string SuggestValidGuidFormat(string invalidGuid)
        {
            if (string.IsNullOrEmpty(invalidGuid))
                return "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

            // Try to fix common issues
            var cleaned = invalidGuid.ToUpper()
                .Replace("G", "0")
                .Replace("H", "1")
                .Replace("I", "2")
                .Replace("J", "3")
                .Replace("K", "4")
                .Replace("L", "5")
                .Replace("M", "6")
                .Replace("N", "7")
                .Replace("O", "8")
                .Replace("P", "9")
                .Replace("Q", "A")
                .Replace("R", "B")
                .Replace("S", "C")
                .Replace("T", "D")
                .Replace("U", "E")
                .Replace("V", "F");

            // Ensure proper format
            if (cleaned.Length == 32)
            {
                return $"{cleaned.Substring(0, 8)}-{cleaned.Substring(8, 4)}-{cleaned.Substring(12, 4)}-{cleaned.Substring(16, 4)}-{cleaned.Substring(20, 12)}";
            }

            return "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        }

        public override async Task<CommentResponse> CreateComment(CommentRequest request, ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogInformation($"CreateComment request received - TaskId: '{request.TaskId}', UserId: '{request.UserId}', Content: '{request.Content?.Substring(0, Math.Min(50, request.Content?.Length ?? 0))}...'");

                if (string.IsNullOrEmpty(request.TaskId))
                {
                    throw new ArgumentException("Task ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.TaskId, out Guid taskId))
                {
                    var suggestion = SuggestValidGuidFormat(request.TaskId);
                    _logger.LogWarning($"Invalid task ID format received: '{request.TaskId}'. Suggested format: {suggestion}");
                    throw new ArgumentException($"Invalid task ID format: {request.TaskId}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx. Suggested: {suggestion}");
                }

                var comment = _mapper.Map<CommentBusinessModel>(request);
                var result = await _commentService.AddCommentAsync(taskId, comment);
                return _mapper.Map<CommentResponse>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for creating comment");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create comment"));
            }
        }

        public override async Task<Empty> DeleteComment(CommentRequestById request, ServerCallContext context)
        {
            try
            {
                Guid commentId = Guid.Parse(request.Id);
                Guid userId = Guid.Parse(request.UserId);
                await _commentService.DeleteCommentAsync(commentId, userId);
                return new Empty();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for deleting comment");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to delete comment"));
            }
        }

        public override async Task<CommentListResponse> GetCommentsByTaskId(CommentRequestByTaskId request,
            ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogInformation($"GetCommentsByTaskId request received - TaskId: '{request.TaskId}', PageSize: {request.PageSize}, PageIndex: {request.PageIndex}");

                if (string.IsNullOrEmpty(request.TaskId))
                {
                    throw new ArgumentException("Task ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.TaskId, out Guid taskId))
                {
                    var suggestion = SuggestValidGuidFormat(request.TaskId);
                    _logger.LogWarning($"Invalid task ID format received: '{request.TaskId}'. Suggested format: {suggestion}");
                    throw new ArgumentException($"Invalid task ID format: {request.TaskId}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx. Suggested: {suggestion}");
                }

                var paginatedComments = await _commentService.GetCommentsByTaskIdWithPaginationAsync(
                    taskId, request.PageSize, request.PageIndex);

                var response = new CommentListResponse();
                foreach (var comment in paginatedComments.Items)
                {
                    response.Comments.Add(_mapper.Map<CommentResponse>(comment));
                }

                response.TotalCount = paginatedComments.Total;
                response.CurrentPage = request.PageIndex;
                response.PageCount = paginatedComments.TotalPages;

                _logger.LogInformation($"Returning {response.Comments.Count} comments out of {response.TotalCount} total");
                return response;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for getting comments");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments by task id");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get comments"));
            }
        }

        public override async Task<CommentResponse> UpdateComment(UpdateCommentRequest request,
            ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogInformation($"UpdateComment request received - CommentId: '{request.Id}', UserId: '{request.UserId}', Content: '{request.Content?.Substring(0, Math.Min(50, request.Content?.Length ?? 0))}...'");

                if (string.IsNullOrEmpty(request.Id))
                {
                    throw new ArgumentException("Comment ID cannot be null or empty");
                }

                if (string.IsNullOrEmpty(request.UserId))
                {
                    throw new ArgumentException("User ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.Id, out Guid commentId))
                {
                    var suggestion = SuggestValidGuidFormat(request.Id);
                    _logger.LogWarning($"Invalid comment ID format received: '{request.Id}'. Suggested format: {suggestion}");
                    throw new ArgumentException($"Invalid comment ID format: {request.Id}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx. Suggested: {suggestion}");
                }

                if (!Guid.TryParse(request.UserId, out Guid userId))
                {
                    var suggestion = SuggestValidGuidFormat(request.UserId);
                    _logger.LogWarning($"Invalid user ID format received: '{request.UserId}'. Suggested format: {suggestion}");
                    throw new ArgumentException($"Invalid user ID format: {request.UserId}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx. Suggested: {suggestion}");
                }

                var comment = _mapper.Map<CommentBusinessModel>(request);
                var result = await _commentService.UpdateCommentAsync(commentId, comment, userId);
                return _mapper.Map<CommentResponse>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for updating comment");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update comment"));
            }
        }
        
    }
}