using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tasco.ProjectService.Service.Protos;

namespace Tasco.Orchestrator.Infrastructure.GrpcClients
{
    public class TaskMemberClientService
    {
        private readonly TaskMemberService.TaskMemberServiceClient _taskMemberClient;
        private readonly ILogger<TaskMemberClientService> _logger;

        public TaskMemberClientService(TaskMemberService.TaskMemberServiceClient taskMemberClient, ILogger<TaskMemberClientService> logger)
        {
            _taskMemberClient = taskMemberClient;
            _logger = logger;
        }

        public async Task<TaskMemberResponse?> CreateTaskMemberAsync(TaskMemberRequest request)
        {
            try
            {
                _logger.LogInformation("Creating task member for task {WorkTaskId} with user {UserId} ({UserName})", 
                    request.WorkTaskId, request.UserId, request.UserName);
                var response = await _taskMemberClient.CreateTaskMemberAsync(request);
                _logger.LogInformation("Successfully created task member with ID {TaskMemberId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to create task member for task {WorkTaskId}", request.WorkTaskId);
                return null;
            }
        }

        public async Task<TaskMemberResponse?> UpdateTaskMemberAsync(UpdateTaskMemberRequest request)
        {
            try
            {
                _logger.LogInformation("Updating task member {TaskMemberId}", request.Id);
                var response = await _taskMemberClient.UpdateTaskMemberAsync(request);
                _logger.LogInformation("Successfully updated task member {TaskMemberId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to update task member {TaskMemberId}", request.Id);
                return null;
            }
        }

        public async Task<TaskMemberDeleteResponse?> DeleteTaskMemberAsync(TaskMemberRequestById request)
        {
            try
            {
                _logger.LogInformation("Deleting task member {TaskMemberId}", request.Id);
                var response = await _taskMemberClient.DeleteTaskMemberAsync(request);
                _logger.LogInformation("Successfully deleted task member {TaskMemberId}: {Message}", 
                    request.Id, response.Message);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to delete task member {TaskMemberId}", request.Id);
                return null;
            }
        }

        public async Task<TaskMemberResponse?> GetTaskMemberByIdAsync(TaskMemberRequestById request)
        {
            try
            {
                _logger.LogInformation("Getting task member {TaskMemberId}", request.Id);
                var response = await _taskMemberClient.GetTaskMemberByIdAsync(request);
                _logger.LogInformation("Successfully retrieved task member {TaskMemberId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get task member {TaskMemberId}", request.Id);
                return null;
            }
        }

        public async Task<TaskMemberListResponse?> GetTaskMembersByTaskIdAsync(TaskMemberRequestByTaskId request)
        {
            try
            {
                _logger.LogInformation("Getting task members for task {WorkTaskId}, page {PageIndex}, size {PageSize}",
                    request.WorkTaskId, request.PageIndex, request.PageSize);
                var response = await _taskMemberClient.GetTaskMembersByTaskIdAsync(request);
                _logger.LogInformation("Successfully retrieved {Count} task members for task {WorkTaskId}",
                    response.TaskMembers.Count, request.WorkTaskId);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to get task members for task {WorkTaskId}", request.WorkTaskId);
                return null;
            }
        }

        public async Task<TaskMemberResponse?> RemoveTaskMemberAsync(RemoveTaskMemberRequest request)
        {
            try
            {
                _logger.LogInformation("Removing task member {TaskMemberId} by user {RemovedByUserId}", 
                    request.Id, request.RemovedByUserId);
                var response = await _taskMemberClient.RemoveTaskMemberAsync(request);
                _logger.LogInformation("Successfully removed task member {TaskMemberId}", response.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to remove task member {TaskMemberId}", request.Id);
                return null;
            }
        }
    }
}
