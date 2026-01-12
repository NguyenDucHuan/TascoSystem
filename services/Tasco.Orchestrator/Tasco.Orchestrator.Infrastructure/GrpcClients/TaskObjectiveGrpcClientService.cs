using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tasco.ProjectService.Service.Protos;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class TaskObjectiveGrpcClientService
    {
        private readonly TaskObjectiveService.TaskObjectiveServiceClient _taskObjectiveClient;
        private readonly ILogger<TaskObjectiveGrpcClientService> _logger;

        public TaskObjectiveGrpcClientService(TaskObjectiveService.TaskObjectiveServiceClient taskObjectiveClient, ILogger<TaskObjectiveGrpcClientService> logger)
        {
            _taskObjectiveClient = taskObjectiveClient;
            _logger = logger;
        }

        public async Task<TaskObjectiveResponse?> CreateTaskObjectiveAsync(TaskObjectiveRequest request)
        {
            try
            {
                _logger.LogInformation("Creating task objective for work task {WorkTaskId} with title {Title}",
                    request.WorkTaskId, request.Title);
                var response = await _taskObjectiveClient.CreateTaskObjectiveAsync(request);
                _logger.LogInformation("Successfully created task objective with ID {TaskObjectiveId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to create task objective for work task {WorkTaskId}", request.WorkTaskId);
                return null;
            }
        }

        public async Task<bool> DeleteTaskObjectiveAsync(TaskObjectiveRequestById request)
        {
            try
            {
                _logger.LogInformation("Deleting task objective {TaskObjectiveId}", request.Id);
                await _taskObjectiveClient.DeleteTaskObjectiveAsync(request);
                _logger.LogInformation("Successfully deleted task objective {TaskObjectiveId}", request.Id);
                return true;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to delete task objective {TaskObjectiveId}", request.Id);
                return false;
            }
        }

        public async Task<TaskObjectiveResponse?> GetTaskObjectiveByIdAsync(TaskObjectiveRequestById request)
        {
            try
            {
                _logger.LogInformation("Getting task objective {TaskObjectiveId}", request.Id);
                var response = await _taskObjectiveClient.GetTaskObjectiveByIdAsync(request);
                _logger.LogInformation("Successfully retrieved task objective {TaskObjectiveId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get task objective {TaskObjectiveId}", request.Id);
                return null;
            }
        }

        public async Task<TaskObjectiveResponse?> UpdateTaskObjectiveAsync(UpdateTaskObjectiveRequest request)
        {
            try
            {
                _logger.LogInformation("Updating task objective {TaskObjectiveId} with title {Title}",
                    request.Id, request.Title);
                var response = await _taskObjectiveClient.UpdateTaskObjectiveAsync(request);
                _logger.LogInformation("Successfully updated task objective {TaskObjectiveId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to update task objective {TaskObjectiveId}", request.Id);
                return null;
            }
        }

        public async Task<TaskObjectiveListResponse?> GetTaskObjectivesByWorkTaskIdAsync(TaskObjectiveRequestByWorkTaskId request)
        {
            try
            {
                _logger.LogInformation("Getting task objectives for work task {WorkTaskId}, page {PageIndex}, size {PageSize}",
                    request.WorkTaskId, request.PageIndex, request.PageSize);
                var response = await _taskObjectiveClient.GetTaskObjectivesByWorkTaskIdAsync(request);
                _logger.LogInformation("Successfully retrieved {Count} task objectives for work task {WorkTaskId}",
                    response.TaskObjectives.Count, request.WorkTaskId);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get task objectives for work task {WorkTaskId}", request.WorkTaskId);
                return null;
            }
        }

        public async Task<TaskObjectiveResponse?> CompleteTaskObjectiveAsync(CompleteTaskObjectiveRequest request)
        {
            try
            {
                _logger.LogInformation("Completing task objective {TaskObjectiveId} with status {IsCompleted}",
                    request.Id, request.IsCompleted);
                var response = await _taskObjectiveClient.CompleteTaskObjectiveAsync(request);
                _logger.LogInformation("Successfully completed task objective {TaskObjectiveId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to complete task objective {TaskObjectiveId}", request.Id);
                return null;
            }
        }
    }
}