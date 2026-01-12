using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.Orchestrator.Api.BussinessModel.TaskMenberController;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/worktasks/{workTaskId}/members")]
    [Authorize]
    public class TaskMenberController : ControllerBase
    {
        private readonly TaskMemberClientService _taskMemberClientService;
        private readonly ILogger<TaskMenberController> _logger;

        public TaskMenberController(
            TaskMemberClientService taskMemberClientService,
            ILogger<TaskMenberController> logger)
        {
            _taskMemberClientService = taskMemberClientService;
            _logger = logger;
        }

        /// <summary>
        /// Thêm thành viên vào task
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="request">Thông tin thành viên</param>
        /// <returns>Kết quả thêm thành viên</returns>
        [HttpPost]
        public async Task<IActionResult> CreateTaskMember(string workTaskId, [FromBody] CreateTaskMemberRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Task member data is required");
                }

                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest("User ID is required");
                }

                if (string.IsNullOrEmpty(workTaskId))
                {
                    return BadRequest("Work Task ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid currentUserId))
                {
                    _logger.LogInformation("Creating task member for task {WorkTaskId} by user {CurrentUserId}",
                        workTaskId, currentUserId);

                    var grpcRequest = new TaskMemberRequest
                    {
                        WorkTaskId = workTaskId,
                        UserId = request.UserId,
                        UserName = request.UserName ?? "",
                        UserEmail = request.UserEmail ?? "",
                        Role = request.Role ?? "MEMBER",
                        AssignedByUserId = currentUserId.ToString()
                    };

                    var response = await _taskMemberClientService.CreateTaskMemberAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Task member added successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to add task member"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task member for task {WorkTaskId}", workTaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách thành viên của task
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="pageIndex">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách thành viên</returns>
        [HttpGet]
        public async Task<IActionResult> GetTaskMembersByTaskId(
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
                    _logger.LogInformation("Getting task members for task {WorkTaskId}, page {PageIndex}, size {PageSize}",
                        workTaskId, pageIndex, pageSize);

                    var request = new TaskMemberRequestByTaskId
                    {
                        WorkTaskId = workTaskId,
                        PageIndex = pageIndex,
                        PageSize = pageSize
                    };

                    var response = await _taskMemberClientService.GetTaskMembersByTaskIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to get task members"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task members for task {WorkTaskId}", workTaskId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin thành viên theo ID
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <returns>Thông tin thành viên</returns>
        [HttpGet("{memberId}")]
        public async Task<IActionResult> GetTaskMemberById(string workTaskId, string memberId)
        {
            try
            {
                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest("Member ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting task member {MemberId} by user {UserId}",
                        memberId, userId);

                    var request = new TaskMemberRequestById
                    {
                        Id = memberId
                    };

                    var response = await _taskMemberClientService.GetTaskMemberByIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Task member not found"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task member {MemberId}", memberId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin thành viên
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{memberId}")]
        public async Task<IActionResult> UpdateTaskMember(string workTaskId, string memberId, [FromBody] UpdateTaskMemberRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Task member data is required");
                }

                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest("Member ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Updating task member {MemberId} by user {UserId}",
                        memberId, userId);

                    var memberRequest = new TaskMemberRequest
                    {
                        WorkTaskId = workTaskId,
                        UserId = request.UserId ?? "",
                        UserName = request.UserName ?? "",
                        UserEmail = request.UserEmail ?? "",
                        Role = request.Role ?? "",
                        AssignedByUserId = userId.ToString()
                    };

                    var grpcRequest = new UpdateTaskMemberRequest
                    {
                        Id = memberId,
                        Member = memberRequest
                    };

                    var response = await _taskMemberClientService.UpdateTaskMemberAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Task member updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update task member"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task member {MemberId}", memberId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Xóa thành viên khỏi task (hard delete)
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{memberId}")]
        public async Task<IActionResult> DeleteTaskMember(string workTaskId, string memberId)
        {
            try
            {
                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest("Member ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Deleting task member {MemberId} by user {UserId}",
                        memberId, userId);

                    var request = new TaskMemberRequestById
                    {
                        Id = memberId
                    };

                    var response = await _taskMemberClientService.DeleteTaskMemberAsync(request);

                    if (response != null && response.Success)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = response.Message
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = response?.Message ?? "Failed to delete task member"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task member {MemberId}", memberId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Loại bỏ thành viên khỏi task (soft delete)
        /// </summary>
        /// <param name="workTaskId">ID của work task</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <returns>Kết quả loại bỏ</returns>
        [HttpPut("{memberId}/remove")]
        public async Task<IActionResult> RemoveTaskMember(string workTaskId, string memberId)
        {
            try
            {
                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest("Member ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Removing task member {MemberId} by user {UserId}",
                        memberId, userId);

                    var request = new RemoveTaskMemberRequest
                    {
                        Id = memberId,
                        RemovedByUserId = userId.ToString()
                    };

                    var response = await _taskMemberClientService.RemoveTaskMemberAsync(request);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Task member removed successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to remove task member"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing task member {MemberId}", memberId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }
    }


}