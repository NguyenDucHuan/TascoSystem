using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tasco.ProjectService.Service.Protos;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class SubTaskGrpcClientService
    {
        private readonly SubTaskService.SubTaskServiceClient _subTaskClient;
        private readonly ILogger<SubTaskGrpcClientService> _logger;

        public SubTaskGrpcClientService(SubTaskService.SubTaskServiceClient subTaskClient, ILogger<SubTaskGrpcClientService> logger)
        {
            _subTaskClient = subTaskClient;
            _logger = logger;
        }

        public async Task<SubTaskResponse?> CreateSubTaskAsync(SubTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Creating subtask for parent task {ParentTaskId} with title {Title}", 
                    request.ParentTaskId, request.Title);
                var response = await _subTaskClient.CreateSubTaskAsync(request);
                _logger.LogInformation("Successfully created subtask with ID {SubTaskId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to create subtask for parent task {ParentTaskId}", request.ParentTaskId);
                return null;
            }
        }

        public async Task<bool> DeleteSubTaskAsync(SubTaskRequestById request)
        {
            try
            {
                _logger.LogInformation("Deleting subtask {SubTaskId}", request.Id);
                await _subTaskClient.DeleteSubTaskAsync(request);
                _logger.LogInformation("Successfully deleted subtask {SubTaskId}", request.Id);
                return true;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to delete subtask {SubTaskId}", request.Id);
                return false;
            }
        }

        public async Task<SubTaskResponse?> GetSubTaskByIdAsync(SubTaskRequestById request)
        {
            try
            {
                _logger.LogInformation("Getting subtask {SubTaskId}", request.Id);
                var response = await _subTaskClient.GetSubTaskByIdAsync(request);
                _logger.LogInformation("Successfully retrieved subtask {SubTaskId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get subtask {SubTaskId}", request.Id);
                return null;
            }
        }

        public async Task<SubTaskResponse?> UpdateSubTaskAsync(UpdateSubTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Updating subtask {SubTaskId} with title {Title}", request.Id, request.Title);
                var response = await _subTaskClient.UpdateSubTaskAsync(request);
                _logger.LogInformation("Successfully updated subtask {SubTaskId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to update subtask {SubTaskId}", request.Id);
                return null;
            }
        }

        public async Task<SubTaskListResponse?> GetSubTasksByTaskObjectiveIdAsync(SubTaskRequestByTaskObjectiveId request)
        {
            try
            {
                _logger.LogInformation("Getting subtasks for parent task {ParentTaskId}, page {PageIndex}, size {PageSize}",
                    request.ParentTaskId, request.PageIndex, request.PageSize);
                var response = await _subTaskClient.GetSubTasksByTaskObjectiveIdAsync(request);
                _logger.LogInformation("Successfully retrieved {Count} subtasks for parent task {ParentTaskId}",
                    response.SubTasks.Count, request.ParentTaskId);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get subtasks for parent task {ParentTaskId}", request.ParentTaskId);
                return null;
            }
        }

        public async Task<SubTaskResponse?> CompleteSubTaskAsync(CompleteSubTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Completing subtask {SubTaskId} with status {IsCompleted}", 
                    request.Id, request.IsCompleted);
                var response = await _subTaskClient.CompleteSubTaskAsync(request);
                _logger.LogInformation("Successfully completed subtask {SubTaskId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to complete subtask {SubTaskId}", request.Id);
                return null;
            }
        }

        public async Task<SubTaskResponse?> AssignSubTaskAsync(AssignSubTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Assigning subtask {SubTaskId} to user {AssigneeId} ({AssigneeName})", 
                    request.Id, request.AssigneeId, request.AssigneeName);
                var response = await _subTaskClient.AssignSubTaskAsync(request);
                _logger.LogInformation("Successfully assigned subtask {SubTaskId} to {AssigneeName}", 
                    response.Id, response.AssigneeName);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to assign subtask {SubTaskId}", request.Id);
                return null;
            }
        }
    }
}