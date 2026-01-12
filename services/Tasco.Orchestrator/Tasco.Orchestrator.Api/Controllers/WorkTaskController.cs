using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.Orchestrator.Api.BussinessModel.WorkTaskModel;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/workareas/{workAreaId}/worktasks")]
    [Authorize]
    public class WorkTaskController : ControllerBase
    {
        private readonly WorkTaskGrpcClientService _workTaskGrpcClient;
        private readonly ILogger<WorkTaskController> _logger;

        public WorkTaskController(
            WorkTaskGrpcClientService workTaskGrpcClient,
            ILogger<WorkTaskController> logger)
        {
            _workTaskGrpcClient = workTaskGrpcClient;
            _logger = logger;
        }

        /// <summary>
        /// Tạo work task mới
        /// </summary>
        /// <param name="workAreaId">ID của work area</param>
        /// <param name="request">Thông tin work task</param>
        /// <returns>Kết quả tạo work task</returns>
        [HttpPost]
        public async Task<IActionResult> CreateWorkTask(string workAreaId, [FromBody] CreateWorkTaskRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Work task data is required");
                }

                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("Work task title is required");
                }

                if (string.IsNullOrEmpty(workAreaId))
                {
                    return BadRequest("Work Area ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Creating work task for work area {WorkAreaId} by user {UserId}",
                        workAreaId, userId);

                    var grpcRequest = new WorkTaskRequest
                    {
                        Title = request.Title,
                        Description = request.Description ?? "",
                        WorkAreaId = workAreaId,
                        Status = request.Status ?? "NEW",
                        Priority = request.Priority ?? "MEDIUM",
                        StartDate = request.StartDate ?? "",
                        EndDate = request.EndDate ?? "",
                        DueDate = request.DueDate ?? "",
                        CreatedByUserId = userId.ToString(),
                        CreatedByUserName = User.FindFirstValue(ClaimTypes.Name) ?? ""
                    };

                    var response = await _workTaskGrpcClient.CreateWorkTaskAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Work task created successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to create work task"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work task for work area {WorkAreaId}", workAreaId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin work task theo ID
        /// </summary>
        /// <param name="workAreaId">ID của work area</param>
        /// <param name="workTaskId">ID của work task</param>
        /// <returns>Thông tin work task</returns>
        [HttpGet("{workTaskId}")]
        public async Task<IActionResult> GetWorkTaskById(string workAreaId, string workTaskId)
        {
            try
            {
                if (string.IsNullOrEmpty(workTaskId))
                {
                    return BadRequest("Work Task ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting work task {WorkTaskId} by user {UserId}",
                        workTaskId, userId);

                    var request = new WorkTaskRequestById
                    {
                        Id = workTaskId
                    };

                    var response = await _workTaskGrpcClient.GetWorkTaskByIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Work task not found"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work task {WorkTaskId}", workTaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Cập nhật work task
        /// </summary>
        /// <param name="workAreaId">ID của work area</param>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{workTaskId}")]
        public async Task<IActionResult> UpdateWorkTask(string workAreaId, string workTaskId, [FromBody] UpdateWorkTaskRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Work task data is required");
                }

                if (string.IsNullOrEmpty(workTaskId))
                {
                    return BadRequest("Work Task ID is required");
                }

                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("Work task title is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Updating work task {WorkTaskId} by user {UserId}",
                        workTaskId, userId);

                    var taskRequest = new WorkTaskRequest
                    {
                        Title = request.Title,
                        Description = request.Description ?? "",
                        WorkAreaId = workAreaId,
                        Status = request.Status ?? "",
                        Priority = request.Priority ?? "",
                        StartDate = request.StartDate ?? "",
                        EndDate = request.EndDate ?? "",
                        DueDate = request.DueDate ?? "",
                        CreatedByUserId = userId.ToString(),
                        CreatedByUserName = User.FindFirstValue(ClaimTypes.Name) ?? ""
                    };

                    var grpcRequest = new UpdateWorkTaskRequest
                    {
                        Id = workTaskId,
                        Task = taskRequest
                    };

                    var response = await _workTaskGrpcClient.UpdateWorkTaskAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Work task updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update work task"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work task {WorkTaskId}", workTaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Xóa work task
        /// </summary>
        /// <param name="workAreaId">ID của work area</param>
        /// <param name="workTaskId">ID của work task</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{workTaskId}")]
        public async Task<IActionResult> DeleteWorkTask(string workAreaId, string workTaskId)
        {
            try
            {
                if (string.IsNullOrEmpty(workTaskId))
                {
                    return BadRequest("Work Task ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Deleting work task {WorkTaskId} by user {UserId}",
                        workTaskId, userId);

                    var request = new WorkTaskRequestById
                    {
                        Id = workTaskId
                    };

                    var success = await _workTaskGrpcClient.DeleteWorkTaskAsync(request);

                    if (success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Work task deleted successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to delete work task"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work task {WorkTaskId}", workTaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }
    }


}