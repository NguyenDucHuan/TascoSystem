using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.Orchestrator.Api.BussinessModel.TaskObjectiveModel;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/worktasks/{workTaskId}/taskobjectives")]
    [Authorize]
    public class TaskObjectiveController : ControllerBase
    {
        private readonly TaskObjectiveGrpcClientService _taskObjectiveGrpcClient;
        private readonly ILogger<TaskObjectiveController> _logger;

        public TaskObjectiveController(
            TaskObjectiveGrpcClientService taskObjectiveGrpcClient,
            ILogger<TaskObjectiveController> logger)
        {
            _taskObjectiveGrpcClient = taskObjectiveGrpcClient;
            _logger = logger;
        }

        /// <summary>
        /// Tạo task objective mới
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="request">Thông tin task objective</param>
        /// <returns>Kết quả tạo task objective</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTaskObjective(string workTaskId, [FromBody] CreateTaskObjectiveRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Task objective data is required");
                }

                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("Task objective title is required");
                }

                if (string.IsNullOrEmpty(workTaskId))
                {
                    return BadRequest("Work Task ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Creating task objective for work task {WorkTaskId} by user {UserId}",
                        workTaskId, userId);

                    var grpcRequest = new TaskObjectiveRequest
                    {
                        WorkTaskId = workTaskId,
                        Title = request.Title,
                        Description = request.Description ?? "",
                        DisplayOrder = request.DisplayOrder
                    };

                    var response = await _taskObjectiveGrpcClient.CreateTaskObjectiveAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Task objective created successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to create task objective"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task objective for work task {WorkTaskId}", workTaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách task objectives theo work task ID
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="pageIndex">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách task objectives</returns>
        [HttpGet]
        public async Task<IActionResult> GetTaskObjectivesByWorkTaskId(
            string workTaskId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(workTaskId))
                {
                    return BadRequest("Work Task ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting task objectives for work task {WorkTaskId}, page {PageIndex}, size {PageSize}",
                        workTaskId, pageIndex, pageSize);

                    var request = new TaskObjectiveRequestByWorkTaskId
                    {
                        WorkTaskId = workTaskId,
                        PageIndex = pageIndex,
                        PageSize = pageSize
                    };

                    var response = await _taskObjectiveGrpcClient.GetTaskObjectivesByWorkTaskIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to get task objectives"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task objectives for work task {WorkTaskId}", workTaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin task objective theo ID
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <returns>Thông tin task objective</returns>
        [HttpGet("{taskObjectiveId}")]
        public async Task<IActionResult> GetTaskObjectiveById(string workTaskId, string taskObjectiveId)
        {
            try
            {
                if (string.IsNullOrEmpty(taskObjectiveId))
                {
                    return BadRequest("Task Objective ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting task objective {TaskObjectiveId} by user {UserId}",
                        taskObjectiveId, userId);

                    var request = new TaskObjectiveRequestById
                    {
                        Id = taskObjectiveId
                    };

                    var response = await _taskObjectiveGrpcClient.GetTaskObjectiveByIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Task objective not found"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task objective {TaskObjectiveId}", taskObjectiveId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Cập nhật task objective
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{taskObjectiveId}")]
        public async Task<IActionResult> UpdateTaskObjective(string workTaskId, string taskObjectiveId, [FromBody] UpdateTaskObjectiveRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Task objective data is required");
                }

                if (string.IsNullOrEmpty(taskObjectiveId))
                {
                    return BadRequest("Task Objective ID is required");
                }

                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("Task objective title is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Updating task objective {TaskObjectiveId} by user {UserId}",
                        taskObjectiveId, userId);

                    var grpcRequest = new UpdateTaskObjectiveRequest
                    {
                        Id = taskObjectiveId,
                        Title = request.Title,
                        Description = request.Description ?? "",
                        DisplayOrder = request.DisplayOrder
                    };

                    var response = await _taskObjectiveGrpcClient.UpdateTaskObjectiveAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Task objective updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update task objective"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task objective {TaskObjectiveId}", taskObjectiveId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Xóa task objective
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{taskObjectiveId}")]
        public async Task<IActionResult> DeleteTaskObjective(string workTaskId, string taskObjectiveId)
        {
            try
            {
                if (string.IsNullOrEmpty(taskObjectiveId))
                {
                    return BadRequest("Task Objective ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Deleting task objective {TaskObjectiveId} by user {UserId}",
                        taskObjectiveId, userId);

                    var request = new TaskObjectiveRequestById
                    {
                        Id = taskObjectiveId
                    };

                    var success = await _taskObjectiveGrpcClient.DeleteTaskObjectiveAsync(request);

                    if (success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Task objective deleted successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to delete task objective"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task objective {TaskObjectiveId}", taskObjectiveId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Hoàn thành task objective
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <param name="request">Thông tin hoàn thành</param>
        /// <returns>Kết quả hoàn thành</returns>
        [HttpPut("{taskObjectiveId}/complete")]
        public async Task<IActionResult> CompleteTaskObjective(string workTaskId, string taskObjectiveId, [FromBody] CompleteTaskObjectiveRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Complete data is required");
                }

                if (string.IsNullOrEmpty(taskObjectiveId))
                {
                    return BadRequest("Task Objective ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Completing task objective {TaskObjectiveId} with status {IsCompleted} by user {UserId}",
                        taskObjectiveId, request.IsCompleted, userId);

                    var grpcRequest = new CompleteTaskObjectiveRequest
                    {
                        Id = taskObjectiveId,
                        IsCompleted = request.IsCompleted,
                        WorkTaskId = request.workTaskId,
                        Title = request.title,
                        Description = request.description ?? "",
                        CompletedByUserId = userId.ToString()
                    };

                    var response = await _taskObjectiveGrpcClient.CompleteTaskObjectiveAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Task objective completion status updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update task objective completion status"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task objective {TaskObjectiveId}", taskObjectiveId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }
    }


}