using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.Orchestrator.Api.BussinessModel.WorkAreaModel;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/workareas")]
    [Authorize]
    public class WorkAreaController : ControllerBase
    {
        private readonly WorkAreaGrpcClientService _workAreaGrpcClient;
        private readonly ILogger<WorkAreaController> _logger;

        public WorkAreaController(
            WorkAreaGrpcClientService workAreaGrpcClient,
            ILogger<WorkAreaController> logger)
        {
            _workAreaGrpcClient = workAreaGrpcClient;
            _logger = logger;
        }

        /// <summary>
        /// Tạo work area mới
        /// </summary>
        /// <param name="projectId">ID của project</param>
        /// <param name="request">Thông tin work area</param>
        /// <returns>Kết quả tạo work area</returns>
        [HttpPost]
        public async Task<IActionResult> CreateWorkArea(string projectId, [FromBody] CreateWorkAreaRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Work area data is required");
                }

                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest("Work area name is required");
                }

                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Creating work area for project {ProjectId} by user {UserId}",
                        projectId, userId);

                    var grpcRequest = new WorkAreaRequest
                    {
                        ProjectId = projectId,
                        Name = request.Name,
                        Description = request.Description ?? "",
                        DisplayOrder = request.DisplayOrder.ToString(),
                        CreateByUserId = userId.ToString()
                    };

                    var response = await _workAreaGrpcClient.CreateWorkAreaAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Work area created successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to create work area"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work area for project {ProjectId}", projectId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách work areas theo project ID
        /// </summary>
        /// <param name="projectId">ID của project</param>
        /// <param name="pageIndex">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách work areas</returns>
        [HttpGet]
        public async Task<IActionResult> GetWorkAreasByProjectId(
            string projectId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting work areas for project {ProjectId}, page {PageIndex}, size {PageSize}",
                        projectId, pageIndex, pageSize);

                    var request = new WorkAreaRequestByProjectId
                    {
                        Id = projectId,
                        PageIndex = pageIndex,
                        PageSize = pageSize
                    };

                    var response = await _workAreaGrpcClient.GetMyWorkAreasByProjectIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to get work areas"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work areas for project {ProjectId}", projectId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Lấy thông tin work area theo ID
        /// </summary>
        /// <param name="projectId">ID của project</param>
        /// <param name="workAreaId">ID của work area</param>
        /// <returns>Thông tin work area</returns>
        [HttpGet("{workAreaId}")]
        public async Task<IActionResult> GetWorkAreaById(string projectId, string workAreaId)
        {
            try
            {
                if (string.IsNullOrEmpty(workAreaId))
                {
                    return BadRequest("Work Area ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting work area {WorkAreaId} by user {UserId}",
                        workAreaId, userId);

                    var request = new WorkAreaRequestById
                    {
                        Id = workAreaId
                    };

                    var response = await _workAreaGrpcClient.GetWorkAreaByIdAsync(request);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = "Work area not found"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting work area {WorkAreaId}", workAreaId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Cập nhật work area
        /// </summary>
        /// <param name="projectId">ID của project</param>
        /// <param name="workAreaId">ID của work area</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{workAreaId}")]
        public async Task<IActionResult> UpdateWorkArea(string projectId, string workAreaId, [FromBody] UpdateWorkAreaRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Work area data is required");
                }

                if (string.IsNullOrEmpty(workAreaId))
                {
                    return BadRequest("Work Area ID is required");
                }

                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest("Work area name is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Updating work area {WorkAreaId} by user {UserId}",
                        workAreaId, userId);

                    var areaRequest = new WorkAreaRequest
                    {
                        ProjectId = projectId,
                        Name = request.Name,
                        Description = request.Description ?? "",
                        DisplayOrder = request.DisplayOrder.ToString(),
                        CreateByUserId = userId.ToString()
                    };

                    var grpcRequest = new UpdateWorkAreaRequest
                    {
                        Id = workAreaId,
                        Area = areaRequest
                    };

                    var response = await _workAreaGrpcClient.UpdateWorkAreaAsync(grpcRequest);

                    if (response != null)
                    {
                        return Ok(new
                        {
                            success = true,
                            message = "Work area updated successfully",
                            data = response
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Failed to update work area"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work area {WorkAreaId}", workAreaId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Xóa work area
        /// </summary>
        /// <param name="projectId">ID của project</param>
        /// <param name="workAreaId">ID của work area</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{workAreaId}")]
        public async Task<IActionResult> DeleteWorkArea(string projectId, string workAreaId)
        {
            try
            {
                if (string.IsNullOrEmpty(workAreaId))
                {
                    return BadRequest("Work Area ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Deleting work area {WorkAreaId} by user {UserId}",
                        workAreaId, userId);

                    var request = new WorkAreaRequestById
                    {
                        Id = workAreaId
                    };

                    var response = await _workAreaGrpcClient.DeleteWorkAreaAsync(request);

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
                            message = response?.Message ?? "Failed to delete work area"
                        });
                    }
                }

                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work area {WorkAreaId}", workAreaId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }
    }


}