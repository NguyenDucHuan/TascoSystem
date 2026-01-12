using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.Orchestrator.Api.BussinessModel.SubTaskModel;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/taskobjectives/{taskObjectiveId}/subtasks")]
    [Authorize]
    public class SubTaskController : ControllerBase
    {
        private readonly SubTaskGrpcClientService _subTaskGrpcClient;
        private readonly ILogger<SubTaskController> _logger;

        public SubTaskController(
            SubTaskGrpcClientService subTaskGrpcClient,
            ILogger<SubTaskController> logger)
        {
            _subTaskGrpcClient = subTaskGrpcClient;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSubTask(string taskObjectiveId, [FromBody] SubTaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("SubTask data is required");
                }

                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("SubTask title is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Creating subtask for task objective {TaskObjectiveId} by user {UserId}",
                        taskObjectiveId, userId);

                    var response = await _subTaskGrpcClient.CreateSubTaskAsync(request);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "SubTask created successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to create subtask"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subtask for task objective {TaskObjectiveId}", taskObjectiveId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubTasksByTaskObjectiveId(
            string taskObjectiveId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(taskObjectiveId))
                {
                    return BadRequest("Task Objective ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting subtasks for task objective {TaskObjectiveId}, page {PageIndex}, size {PageSize}",
                        taskObjectiveId, pageIndex, pageSize);

                    var request = new SubTaskRequestByTaskObjectiveId
                    {
                        ParentTaskId = taskObjectiveId,
                        PageIndex = pageIndex,
                        PageSize = pageSize
                    };

                    var response = await _subTaskGrpcClient.GetSubTasksByTaskObjectiveIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to get subtasks"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subtasks for task objective {TaskObjectiveId}", taskObjectiveId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }


        [HttpGet("{subtaskId}")]
        public async Task<IActionResult> GetSubTaskById(string taskObjectiveId, string subtaskId)
        {
            try
            {
                if (string.IsNullOrEmpty(subtaskId))
                {
                    return BadRequest("SubTask ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting subtask {SubTaskId} by user {UserId}",
                        subtaskId, userId);

                    var request = new SubTaskRequestById
                    {
                        Id = subtaskId
                    };

                    var response = await _subTaskGrpcClient.GetSubTaskByIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "SubTask not found"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subtask {SubTaskId}", subtaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Cập nhật subtask
        /// </summary>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <param name="subtaskId">ID của subtask</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{subtaskId}")]
        public async Task<IActionResult> UpdateSubTask(string taskObjectiveId, string subtaskId, [FromBody] UpdateSubTaskRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("SubTask data is required");
                }

                if (string.IsNullOrEmpty(subtaskId))
                {
                    return BadRequest("SubTask ID is required");
                }

                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("SubTask title is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Updating subtask {SubTaskId} by user {UserId}",
                        subtaskId, userId);

                    var grpcRequest = new UpdateSubTaskRequest
                    {
                        Id = subtaskId,
                        Title = request.Title,
                        Description = request.Description ?? "",
                        Status = request.Status
                    };

                    var response = await _subTaskGrpcClient.UpdateSubTaskAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "SubTask updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update subtask"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subtask {SubTaskId}", subtaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Xóa subtask
        /// </summary>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <param name="subtaskId">ID của subtask</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{subtaskId}")]
        public async Task<IActionResult> DeleteSubTask(string taskObjectiveId, string subtaskId)
        {
            try
            {
                if (string.IsNullOrEmpty(subtaskId))
                {
                    return BadRequest("SubTask ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Deleting subtask {SubTaskId} by user {UserId}",
                        subtaskId, userId);

                    var request = new SubTaskRequestById
                    {
                        Id = subtaskId
                    };

                    var success = await _subTaskGrpcClient.DeleteSubTaskAsync(request);

                    if (success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "SubTask deleted successfully"
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to delete subtask"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subtask {SubTaskId}", subtaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Hoàn thành subtask
        /// </summary>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <param name="subtaskId">ID của subtask</param>
        /// <param name="request">Thông tin hoàn thành</param>
        /// <returns>Kết quả hoàn thành</returns>
        [HttpPut("{subtaskId}/complete")]
        public async Task<IActionResult> CompleteSubTask(string taskObjectiveId, string subtaskId, [FromBody] CompleteSubTaskRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Complete data is required");
                }

                if (string.IsNullOrEmpty(subtaskId))
                {
                    return BadRequest("SubTask ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Completing subtask {SubTaskId} with status {IsCompleted} by user {UserId}",
                        subtaskId, request.IsCompleted, userId);

                    var grpcRequest = new CompleteSubTaskRequest
                    {
                        Id = subtaskId,
                        IsCompleted = request.IsCompleted
                    };

                    var response = await _subTaskGrpcClient.CompleteSubTaskAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "SubTask completion status updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update subtask completion status"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing subtask {SubTaskId}", subtaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Gán subtask cho người dùng
        /// </summary>
        /// <param name="taskObjectiveId">ID của task objective</param>
        /// <param name="subtaskId">ID của subtask</param>
        /// <param name="request">Thông tin gán</param>
        /// <returns>Kết quả gán</returns>
        [HttpPut("{subtaskId}/assign")]
        public async Task<IActionResult> AssignSubTask(string taskObjectiveId, string subtaskId, [FromBody] AssignSubTaskRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Assign data is required");
                }

                if (string.IsNullOrEmpty(subtaskId))
                {
                    return BadRequest("SubTask ID is required");
                }

                if (string.IsNullOrEmpty(request.AssigneeId))
                {
                    return BadRequest("Assignee ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Assigning subtask {SubTaskId} to user {AssigneeId} by user {UserId}",
                        subtaskId, request.AssigneeId, userId);

                    var grpcRequest = new AssignSubTaskRequest
                    {
                        Id = subtaskId,
                        AssigneeId = request.AssigneeId,
                        AssigneeName = request.AssigneeName ?? ""
                    };

                    var response = await _subTaskGrpcClient.AssignSubTaskAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "SubTask assigned successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to assign subtask"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning subtask {SubTaskId}", subtaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }
    }


}