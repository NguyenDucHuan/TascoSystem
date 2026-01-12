using Microsoft.Extensions.Logging;
using Grpc.Core;
using Tasco.ProjectService.Service.Protos;

namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class WorkAreaGrpcClientService
    {
        private readonly WorkAreaService.WorkAreaServiceClient _workAreaClient;
        private readonly ILogger<WorkAreaGrpcClientService> _logger;

        public WorkAreaGrpcClientService(WorkAreaService.WorkAreaServiceClient workAreaClient, ILogger<WorkAreaGrpcClientService> logger)
        {
            _workAreaClient = workAreaClient;
            _logger = logger;
        }

        public async Task<WorkAreaResponse?> CreateWorkAreaAsync(WorkAreaRequest request)
        {
            try
            {
                _logger.LogInformation("Creating work area for project {ProjectId} with name {Name}",
                    request.ProjectId, request.Name);
                var response = await _workAreaClient.CreateWorkAreaAsync(request);
                _logger.LogInformation("Successfully created work area with ID {WorkAreaId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to create work area for project {ProjectId}", request.ProjectId);
                return null;
            }
        }

        public async Task<DeleteResponse?> DeleteWorkAreaAsync(WorkAreaRequestById request)
        {
            try
            {
                _logger.LogInformation("Deleting work area {WorkAreaId}", request.Id);
                var response = await _workAreaClient.DeleteWorkAreaAsync(request);
                _logger.LogInformation("Successfully deleted work area {WorkAreaId}: {Message}",
                    request.Id, response.Message);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to delete work area {WorkAreaId}", request.Id);
                return null;
            }
        }

        public async Task<WorkAreaResponse?> GetWorkAreaByIdAsync(WorkAreaRequestById request)
        {
            try
            {
                _logger.LogInformation("Getting work area {WorkAreaId}", request.Id);
                var response = await _workAreaClient.GetWorkAreaByIdAsync(request);
                _logger.LogInformation("Successfully retrieved work area {WorkAreaId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get work area {WorkAreaId}", request.Id);
                return null;
            }
        }

        public async Task<WorkAreaResponse?> UpdateWorkAreaAsync(UpdateWorkAreaRequest request)
        {
            try
            {
                _logger.LogInformation("Updating work area {WorkAreaId} with name {Name}",
                    request.Id, request.Area.Name);
                var response = await _workAreaClient.UpdateWorkAreaAsync(request);
                _logger.LogInformation("Successfully updated work area {WorkAreaId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to update work area {WorkAreaId}", request.Id);
                return null;
            }
        }

        public async Task<WorkAreaListResponse?> GetMyWorkAreasByProjectIdAsync(WorkAreaRequestByProjectId request)
        {
            try
            {
                _logger.LogInformation("Getting work areas for project {ProjectId}, page {PageIndex}, size {PageSize}",
                    request.Id, request.PageIndex, request.PageSize);
                var response = await _workAreaClient.GetMyWorkAreasByProjectIdAsync(request);
                _logger.LogInformation("Successfully retrieved {Count} work areas for project {ProjectId}",
                    response.WorkAreas.Count, request.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get work areas for project {ProjectId}", request.Id);
                return null;
            }
        }
    }
}