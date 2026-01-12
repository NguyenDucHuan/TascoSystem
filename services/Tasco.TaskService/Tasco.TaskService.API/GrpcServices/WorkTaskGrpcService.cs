using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.BusinessModels;
using Tasco.TaskService.Service.Interfaces;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace Tasco.TaskService.API.GrpcServices
{
    public class WorkTaskGrpcService : WorkTaskService.WorkTaskServiceBase
    {
        private readonly IWorkTaskService _workTaskService;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkTaskGrpcService> _logger;

        public WorkTaskGrpcService(
            IWorkTaskService workTaskService,
            IMapper mapper,
            ILogger<WorkTaskGrpcService> logger)
        {
            _workTaskService = workTaskService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<WorkTaskResponseUnique> CreateWorkTask(WorkTaskRequest request,
            ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Creating work task with title: {Title}", request.Title);

                var model = _mapper.Map<WorkTaskBusinessModel>(request);
                var result = await _workTaskService.CreateWorkTask(model);
                return _mapper.Map<WorkTaskResponseUnique>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input data for work task creation: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work task: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the work task"));
            }
        }

        public override async Task<Empty> DeleteWorkTask(WorkTaskRequestById request, ServerCallContext context)
        {
            Guid taskId = Guid.Parse(request.Id);
            await _workTaskService.DeleteWorkTask(taskId);
            return new Empty();
        }

        public override async Task<WorkTaskResponseUnique> GetWorkTaskById(WorkTaskRequestById request,
            ServerCallContext context)
        {
            Guid taskId = Guid.Parse(request.Id);
            var result = await _workTaskService.GetWorkTaskById(taskId);
            return _mapper.Map<WorkTaskResponseUnique>(result);
        }

        public override async Task<WorkTaskResponseUnique> UpdateWorkTask(UpdateWorkTaskRequest request,
            ServerCallContext context)
        {
            try
            {
                if (request?.Task == null)
                {
                    throw new ArgumentException("Task data is required for update");
                }

                _logger.LogInformation("Updating work task with ID: {TaskId}", request.Id);

                Guid taskId = Guid.Parse(request.Id);
                var model = _mapper.Map<WorkTaskBusinessModel>(request.Task);

                if (model == null)
                {
                    throw new InvalidOperationException("Failed to map task data");
                }

                await _workTaskService.UpdateWorkTask(taskId, model);
                var result = await _workTaskService.GetWorkTaskById(taskId);

                return _mapper.Map<WorkTaskResponseUnique>(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid input data for work task update: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work task: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the work task"));
            }
        }
    }
}