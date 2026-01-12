using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.Interfaces;
using Tasco.TaskService.Service.BusinessModels;

namespace Tasco.TaskService.API.GrpcServices
{
    public class TaskMemberGrpcService : TaskMemberService.TaskMemberServiceBase
    {
        private readonly ITaskMemberService _taskMemberService;
        private readonly IMapper _mapper;
        private readonly ILogger<TaskMemberGrpcService> _logger;

        public TaskMemberGrpcService(
            ITaskMemberService taskMemberService,
            IMapper mapper,
            ILogger<TaskMemberGrpcService> logger)
        {
            _taskMemberService = taskMemberService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<TaskMemberResponse> CreateTaskMember(TaskMemberRequest request, ServerCallContext context)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogInformation($"Creating task member for task ID: {request.WorkTaskId}, User ID: {request.UserId}");

                var model = _mapper.Map<TaskMemberBusinessModel>(request);
                var result = await _taskMemberService.CreateTaskMember(model);
                return _mapper.Map<TaskMemberResponse>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input parameters for creating task member");
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while creating task member");
                throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task member");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to create task member"));
            }
        }

        public override async Task<TaskMemberResponse> UpdateTaskMember(UpdateTaskMemberRequest request, ServerCallContext context)
        {
            Guid taskMemberId = Guid.Parse(request.Id);
            try
            {
                var model = _mapper.Map<TaskMemberBusinessModel>(request.Member);
                if (model == null)
                {
                    throw new InvalidOperationException("Failed to map request to business model");
                }
                
                await _taskMemberService.UpdateTaskMember(taskMemberId, model);

                var updatedTaskMember = await _taskMemberService.GetTaskMemberById(taskMemberId);
                return _mapper.Map<TaskMemberResponse>(updatedTaskMember);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task member with ID {TaskMemberId}", taskMemberId);
                throw;
            }
        }

        public override async Task<TaskMemberDeleteResponse> DeleteTaskMember(TaskMemberRequestById request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.Id))
            {
                throw new ArgumentException("Task member ID cannot be null or empty");
            }

            Guid taskMemberId = Guid.Parse(request.Id);
            await _taskMemberService.DeleteTaskMember(taskMemberId);

            return new TaskMemberDeleteResponse
            {
                Message = "Task member deleted successfully",
                Success = true
            };
        }

        public override async Task<TaskMemberResponse> GetTaskMemberById(TaskMemberRequestById request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.Id))
            {
                throw new ArgumentException("Task member ID cannot be null or empty");
            }

            Guid taskMemberId = Guid.Parse(request.Id);
            var result = await _taskMemberService.GetTaskMemberById(taskMemberId);
            return _mapper.Map<TaskMemberResponse>(result);
        }

        public override async Task<TaskMemberListResponse> GetTaskMembersByTaskId(TaskMemberRequestByTaskId request, ServerCallContext context)
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
                throw new ArgumentException("Invalid work task ID format");
            }

            int pageSize = request.PageSize;
            int pageIndex = request.PageIndex;

            var result = await _taskMemberService.GetTaskMembersByTaskId(pageSize, pageIndex, workTaskId);
            return _mapper.Map<TaskMemberListResponse>(result);
        }

        public override async Task<TaskMemberResponse> RemoveTaskMember(RemoveTaskMemberRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.Id))
            {
                throw new ArgumentException("Task member ID cannot be null or empty");
            }

            if (string.IsNullOrEmpty(request.RemovedByUserId))
            {
                throw new ArgumentException("Removed by user ID cannot be null or empty");
            }

            if (!Guid.TryParse(request.Id, out Guid taskMemberId))
            {
                throw new ArgumentException("Invalid task member ID format");
            }

            if (!Guid.TryParse(request.RemovedByUserId, out Guid removedByUserId))
            {
                throw new ArgumentException("Invalid removed by user ID format");
            }

            _logger.LogInformation($"Removing task member with ID: {request.Id}");

            try
            {
                await _taskMemberService.RemoveTaskMember(taskMemberId, removedByUserId);

                var removedTaskMember = await _taskMemberService.GetTaskMemberById(taskMemberId);
                return _mapper.Map<TaskMemberResponse>(removedTaskMember);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing task member with ID {TaskMemberId}", taskMemberId);
                throw;
            }
        }
    }
} 