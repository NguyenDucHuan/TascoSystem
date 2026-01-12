using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tasco.ProjectService.Service.Protos;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class CommentGrpcClientService
    {
        private readonly CommentService.CommentServiceClient _commentClient;
        private readonly ILogger<CommentGrpcClientService> _logger;

        public CommentGrpcClientService(CommentService.CommentServiceClient commentClient, ILogger<CommentGrpcClientService> logger)
        {
            _commentClient = commentClient;
            _logger = logger;
        }

        public async Task<CommentResponse?> CreateCommentAsync(CommentRequest request)
        {
            try
            {
                _logger.LogInformation("Creating comment for task {TaskId} by user {UserId}", request.TaskId, request.UserId);
                var response = await _commentClient.CreateCommentAsync(request);
                _logger.LogInformation("Successfully created comment with ID {CommentId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to create comment for task {TaskId}", request.TaskId);
                return null;
            }
        }

        public async Task<bool> DeleteCommentAsync(CommentRequestById request)
        {
            try
            {
                _logger.LogInformation("Deleting comment {CommentId} by user {UserId}", request.Id, request.UserId);
                await _commentClient.DeleteCommentAsync(request);
                _logger.LogInformation("Successfully deleted comment {CommentId}", request.Id);
                return true;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to delete comment {CommentId}", request.Id);
                return false;
            }
        }

        public async Task<CommentListResponse?> GetCommentsByTaskIdAsync(CommentRequestByTaskId request)
        {
            try
            {
                _logger.LogInformation("Getting comments for task {TaskId}, page {PageIndex}, size {PageSize}",
                    request.TaskId, request.PageIndex, request.PageSize);
                var response = await _commentClient.GetCommentsByTaskIdAsync(request);
                _logger.LogInformation("Successfully retrieved {Count} comments for task {TaskId}",
                    response.Comments.Count, request.TaskId);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get comments for task {TaskId}", request.TaskId);
                return null;
            }
        }

        public async Task<CommentResponse?> UpdateCommentAsync(UpdateCommentRequest request)
        {
            try
            {
                _logger.LogInformation("Updating comment {CommentId} by user {UserId}", request.Id, request.UserId);
                var response = await _commentClient.UpdateCommentAsync(request);
                _logger.LogInformation("Successfully updated comment {CommentId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to update comment {CommentId}", request.Id);
                return null;
            }
        }
    }
}