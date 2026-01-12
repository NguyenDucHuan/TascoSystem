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
using Tasco.TaskService.Service.Implementations;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace Tasco.TaskService.API.GrpcServices
{
    public class TaskObjectiveGrpcService : Protos.TaskObjectiveService.TaskObjectiveServiceBase
    {
        private readonly ITaskObjectiveService _taskObjectiveService;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskObjectiveGrpcService> _logger;

        public TaskObjectiveGrpcService(
            ITaskObjectiveService taskObjectiveService,
            IMapper mapper,
            ILogger<TaskObjectiveGrpcService> logger)
        {
            _taskObjectiveService = taskObjectiveService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<TaskObjectiveResponse> CreateTaskObjective(TaskObjectiveRequest request,
            ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogInformation($"CreateTaskObjective request received - WorkTaskId: '{request.WorkTaskId}', Title: '{request.Title}'");

                if (string.IsNullOrEmpty(request.WorkTaskId))
                {
                    throw new ArgumentException("Work task ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.WorkTaskId, out Guid workTaskId))
                {
                    _logger.LogWarning($"Invalid work task ID format received: '{request.WorkTaskId}'");
                    throw new ArgumentException($"Invalid work task ID format: {request.WorkTaskId}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
                }

                var model = new TaskObjectiveBusinessModel
                {
                    WorkTaskId = workTaskId,
                    Title = request.Title,
                    Description = request.Description,
                    DisplayOrder = request.DisplayOrder
                };

                var result = await _taskObjectiveService.CreateTaskObjectiveAsync(model);
                return _mapper.Map<TaskObjectiveResponse>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for creating task objective");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task objective");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create task objective"));
            }
        }

        public override async Task<Empty> DeleteTaskObjective(TaskObjectiveRequestById request,
            ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (string.IsNullOrEmpty(request.Id))
                {
                    throw new ArgumentException("Task objective ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.Id, out Guid taskObjectiveId))
                {
                    _logger.LogWarning($"Invalid task objective ID format received: '{request.Id}'");
                    throw new ArgumentException($"Invalid task objective ID format: {request.Id}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
                }

                await _taskObjectiveService.DeleteTaskObjectiveAsync(taskObjectiveId);
                return new Empty();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for deleting task objective");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task objective");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to delete task objective"));
            }
        }

        public override async Task<TaskObjectiveResponse> GetTaskObjectiveById(TaskObjectiveRequestById request,
            ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (string.IsNullOrEmpty(request.Id))
                {
                    throw new ArgumentException("Task objective ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.Id, out Guid taskObjectiveId))
                {
                    _logger.LogWarning($"Invalid task objective ID format received: '{request.Id}'");
                    throw new ArgumentException($"Invalid task objective ID format: {request.Id}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
                }

                var result = await _taskObjectiveService.GetTaskObjectiveByIdAsync(taskObjectiveId);
                var response = _mapper.Map<TaskObjectiveResponse>(result);
                return response;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for getting task objective");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task objective by id");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get task objective"));
            }
        }

        public override async Task<TaskObjectiveResponse> UpdateTaskObjective(UpdateTaskObjectiveRequest request,
            ServerCallContext context)
        {
            try
            {
                Guid.TryParse(request.Id, out Guid taskObjectiveId);
                Guid.TryParse(request.WorkTaskId, out Guid workTaskId);

                var taskObjective = new TaskObjectiveBusinessModel
                {
                    Id = taskObjectiveId,
                    WorkTaskId = workTaskId,
                    Title = request.Title,
                    Description = request.Description,
                    IsCompleted = request.IsCompleted,
                    DisplayOrder = request.DisplayOrder,
                    CompletedByUserId = Guid.Parse(request.CompletedByUserId)
                };

                var result = await _taskObjectiveService.UpdateTaskObjectiveAsync(taskObjectiveId, taskObjective);
                return _mapper.Map<TaskObjectiveResponse>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for updating task objective");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Task objective not found");
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task objective");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to update task objective"));
            }
        }

        public override async Task<TaskObjectiveListResponse> GetTaskObjectivesByWorkTaskId(
            TaskObjectiveRequestByWorkTaskId request, ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                if (string.IsNullOrEmpty(request.WorkTaskId))
                {
                    throw new ArgumentException("Work task ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.WorkTaskId, out Guid workTaskId))
                {
                    _logger.LogWarning($"Invalid work task ID format received: '{request.WorkTaskId}'");
                    throw new ArgumentException($"Invalid work task ID format: {request.WorkTaskId}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
                }

                var taskObjectives = await _taskObjectiveService.GetTaskObjectivesByWorkTaskIdAsync(workTaskId, request.PageIndex, request.PageSize);
                
                // Map the paginated result to TaskObjectiveListResponse
                var response = new TaskObjectiveListResponse
                {
                    TotalCount = taskObjectives.Total,
                    PageCount = taskObjectives.TotalPages,
                    CurrentPage = taskObjectives.Page
                };

                // Map each TaskObjective to TaskObjectiveResponse
                foreach (var taskObjective in taskObjectives.Items)
                {
                    var taskObjectiveResponse = _mapper.Map<TaskObjectiveResponse>(taskObjective);
                    response.TaskObjectives.Add(taskObjectiveResponse);
                }

                return response;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for getting task objectives");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task objectives by work task id");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to get task objectives"));
            }
        }

        public override async Task<TaskObjectiveResponse> CompleteTaskObjective(CompleteTaskObjectiveRequest request,
            ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogInformation($"CompleteTaskObjective request received - Id: '{request.Id}', CompletedByUserId: '{request.CompletedByUserId}'");

                if (string.IsNullOrEmpty(request.Id))
                {
                    throw new ArgumentException("Task objective ID cannot be null or empty");
                }

                if (string.IsNullOrEmpty(request.CompletedByUserId))
                {
                    throw new ArgumentException("Completed by user ID cannot be null or empty");
                }

                if (!Guid.TryParse(request.Id, out Guid taskObjectiveId))
                {
                    _logger.LogWarning($"Invalid task objective ID format received: '{request.Id}'");
                    throw new ArgumentException($"Invalid task objective ID format: {request.Id}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
                }

                if (!Guid.TryParse(request.CompletedByUserId, out Guid userId))
                {
                    _logger.LogWarning($"Invalid completed by user ID format received: '{request.CompletedByUserId}'");
                    throw new ArgumentException($"Invalid completed by user ID format: {request.CompletedByUserId}. Expected format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
                }

                var result = await _taskObjectiveService.CompleteTaskObjectiveAsync(taskObjectiveId, request.IsCompleted, userId);
                return _mapper.Map<TaskObjectiveResponse>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for completing task objective");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task objective");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to complete task objective"));
            }
        }
    }
}