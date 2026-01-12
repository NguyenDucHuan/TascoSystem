using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tasco.TaskService.API.Protos;
using Tasco.TaskService.Service.Interfaces;
using Tasco.TaskService.Service.BusinessModels;
using WorkTaskResponse = Tasco.TaskService.API.Protos.WorkTaskResponse;
using Newtonsoft.Json;

namespace Tasco.TaskService.API.GrpcServices
{
    public class WorkAreaGrpcService : WorkAreaService.WorkAreaServiceBase
    {
        private readonly IWorkAreaService _workAreaService;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkAreaGrpcService> _logger;

        public WorkAreaGrpcService(
            IWorkAreaService workAreaService,
            IMapper mapper,
            ILogger<WorkAreaGrpcService> logger)
        {
            _workAreaService = workAreaService;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<WorkAreaResponse> CreateWorkArea(WorkAreaRequest request, ServerCallContext context)
        {
            var model = _mapper.Map<WorkAreaBusinessModel>(request);
            var result = await _workAreaService.CreateWorkArea(model);
            return _mapper.Map<WorkAreaResponse>(result);
        }

        public override async Task<DeleteResponse> DeleteWorkArea(WorkAreaRequestById request, ServerCallContext context)
        {
            Guid workAreaId = Guid.Parse(request.Id);   
            await _workAreaService.DeleteWorkArea(workAreaId);
            return new DeleteResponse
            {
                Message = "Delete success",
                Success = true
            };

        }

        public override async Task<WorkAreaResponse> GetWorkAreaById(WorkAreaRequestById request,
            ServerCallContext context)
        {
            Guid workAreaId = Guid.Parse(request.Id);
            var result = await _workAreaService.GetWorkAreaById(workAreaId);
            var response = _mapper.Map<WorkAreaResponse>(result);
            
            // Map navigation properties
            foreach (var workTask in result.WorkTasks)
            {
                var workTaskResponse = _mapper.Map<WorkTaskResponse>(workTask);
                
                // Map TaskActions
                foreach (var taskAction in workTask.TaskActions)
                {
                    workTaskResponse.TaskActions.Add(_mapper.Map<WorkAreaTaskActionResponse>(taskAction));
                }
                
                // Map TaskMembers
                foreach (var taskMember in workTask.TaskMembers)
                {
                    workTaskResponse.TaskMembers.Add(_mapper.Map<WorkAreaTaskMemberResponse>(taskMember));
                }
                
                // Map TaskObjectives with SubTasks
                foreach (var taskObjective in workTask.TaskObjectives)
                {
                    var taskObjectiveResponse = _mapper.Map<WorkAreaTaskObjectiveResponse>(taskObjective);
                    workTaskResponse.TaskObjectives.Add(taskObjectiveResponse);
                }
                
                response.WorkTasks.Add(workTaskResponse);
            }
            
            return response;
        }

        public override async Task<WorkAreaResponse> UpdateWorkArea(UpdateWorkAreaRequest request,
            ServerCallContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(request.Id))
            {
                throw new ArgumentException("Work area ID cannot be null or empty");
            }

            _logger.LogInformation($"Updating work area with ID {request.Id}");
            _logger.LogInformation($"Work area grpc service: {JsonConvert.SerializeObject(request)}");
            
            if (!Guid.TryParse(request.Id, out Guid workAreaId))
            {
                throw new ArgumentException("Invalid work area ID format");
            }

            try
            {
                var model = _mapper.Map<WorkAreaBusinessModel>(request);
                if (model == null)
                {
                    throw new InvalidOperationException("Failed to map request to business model");
                }

                model.Id = workAreaId; // Set the ID from the request
                
                await _workAreaService.UpdateWorkArea(workAreaId, model);
                
                // Get the updated work area and return it
                var updatedWorkArea = await _workAreaService.GetWorkAreaById(workAreaId);
                return _mapper.Map<WorkAreaResponse>(updatedWorkArea);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"Invalid data format: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work area with ID {WorkAreaId}", workAreaId);
                throw;
            }
        }

        public override async Task<WorkAreaListResponse> GetMyWorkAreasByProjectId(WorkAreaRequestByProjectId request,
            ServerCallContext context)
        {
            Guid projectId = Guid.Parse(request.Id);
            int pageSize = request.PageSize;
            int pageIndex = request.PageIndex;
            
            var result = await _workAreaService.GetMyWorkAreasByProjectId(pageSize, pageIndex, projectId);
            var response = _mapper.Map<WorkAreaListResponse>(result);
            
            return response;
        }
    }
}