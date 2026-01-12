using Microsoft.Extensions.Logging;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Tasco.ProjectService.Service.Protos;

namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class WorkTaskGrpcClientService
    {
        private readonly WorkTaskService.WorkTaskServiceClient _workTaskClient;
        private readonly ILogger<WorkTaskGrpcClientService> _logger;

        public WorkTaskGrpcClientService(WorkTaskService.WorkTaskServiceClient workTaskClient, ILogger<WorkTaskGrpcClientService> logger)
        {
            _workTaskClient = workTaskClient;
            _logger = logger;
        }

        public async Task<WorkTaskResponseUnique?> CreateWorkTaskAsync(WorkTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Creating work task for work area {WorkAreaId} with title {Title}",
                    request.WorkAreaId, request.Title);
                var response = await _workTaskClient.CreateWorkTaskAsync(request);
                _logger.LogInformation("Successfully created work task with ID {WorkTaskId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to create work task for work area {WorkAreaId}", request.WorkAreaId);
                return null;
            }
        }

        public async Task<bool> DeleteWorkTaskAsync(WorkTaskRequestById request)
        {
            try
            {
                _logger.LogInformation("Deleting work task {WorkTaskId}", request.Id);
                await _workTaskClient.DeleteWorkTaskAsync(request);
                _logger.LogInformation("Successfully deleted work task {WorkTaskId}", request.Id);
                return true;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to delete work task {WorkTaskId}", request.Id);
                return false;
            }
        }

        public async Task<WorkTaskResponseUnique?> GetWorkTaskByIdAsync(WorkTaskRequestById request)
        {
            try
            {
                _logger.LogInformation("Getting work task {WorkTaskId}", request.Id);
                var response = await _workTaskClient.GetWorkTaskByIdAsync(request);
                _logger.LogInformation("Successfully retrieved work task {WorkTaskId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get work task {WorkTaskId}", request.Id);
                return null;
            }
        }

        public async Task<WorkTaskResponseUnique?> UpdateWorkTaskAsync(UpdateWorkTaskRequest request)
        {
            try
            {
                _logger.LogInformation("Updating work task {WorkTaskId} with title {Title}",
                    request.Id, request.Task.Title);
                var response = await _workTaskClient.UpdateWorkTaskAsync(request);
                _logger.LogInformation("Successfully updated work task {WorkTaskId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to update work task {WorkTaskId}", request.Id);
                return null;
            }
        }
    }
}