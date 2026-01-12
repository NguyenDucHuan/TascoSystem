using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.ProjectService.Service.Protos;
using Tasco.Orchestrator.Api.BussinessModel.ProjectModel;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/members")]
    public class ProjectMenberController : ControllerBase
    {
        private readonly ProjectMenberGrpcClientService _projectMemberGrpcClient;
        private readonly ILogger<ProjectMenberController> _logger;

        public ProjectMenberController(
            ProjectMenberGrpcClientService projectMemberGrpcClient,
            ILogger<ProjectMenberController> logger)
        {
            _projectMemberGrpcClient = projectMemberGrpcClient;
            _logger = logger;
        }

        /// <summary>
        /// Xóa thành viên khỏi dự án
        /// </summary>
        /// <param name="projectId">ID của dự án</param>
        /// <param name="memberId">ID của thành viên cần xóa</param>
        /// <returns>Kết quả xóa thành viên</returns>
        [HttpDelete("{memberId}")]
        public async Task<IActionResult> RemoveMemberFromProject(string projectId, string memberId)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }

                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest("Member ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid currentUserId))
                {
                    _logger.LogInformation("Removing member {MemberId} from project {ProjectId} by user {UserId}",
                    memberId, projectId, currentUserId);

                    var response = await _projectMemberGrpcClient.RemoveMemberFromProjectAsync(
                        projectId, memberId, currentUserId.ToString());

                    if (response.Success)
                    {
                        return Ok(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member {MemberId} from project {ProjectId}", memberId, projectId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    statusCode = 500
                });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái phê duyệt thành viên
        /// </summary>
        /// <param name="projectId">ID của dự án</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <param name="request">Thông tin cập nhật trạng thái</param>
        /// <returns>Kết quả cập nhật trạng thái</returns>
        [HttpPut("{memberId}/approved-status")]
        public async Task<IActionResult> UpdateApprovedStatus(
            string projectId,
            string memberId,
            [FromBody] UpdateApprovedStatusRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }

                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest("Member ID is required");
                }

                if (request == null || string.IsNullOrEmpty(request.ApprovedStatus))
                {
                    return BadRequest("Approved status is required");
                }

                // Validate approved status values
                var validStatuses = new[] { "APPROVED", "REJECTED" };
                if (!validStatuses.Contains(request.ApprovedStatus.ToUpper()))
                {
                    return BadRequest("Invalid approved status. Valid values are: APPROVED, REJECTED");
                }
                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Updating approved status for member {MemberId} in project {ProjectId} to {Status} by user {UserId}",
                    memberId, projectId, request.ApprovedStatus, userId);

                    var response = await _projectMemberGrpcClient.UpdateApprovedStatusAsync(
                        projectId,
                        memberId,
                        "email cho thang approved",
                        "name cho thang approved",
                        request.ApprovedStatus.ToUpper(),
                        userId.ToString());

                    if (response.Success)
                    {
                        return Ok(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating approved status for member {MemberId} in project {ProjectId}", memberId, projectId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    statusCode = 500
                });
            }
        }

        /// <summary>
        /// Cập nhật vai trò thành viên
        /// </summary>
        /// <param name="projectId">ID của dự án</param>
        /// <param name="memberId">ID của thành viên</param>
        /// <param name="request">Thông tin cập nhật vai trò</param>
        /// <returns>Kết quả cập nhật vai trò</returns>
        [HttpPut("{memberId}/role")]
        public async Task<IActionResult> UpdateMemberRole(
            string projectId,
            string memberId,
            [FromBody] UpdateMemberRoleRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }

                if (string.IsNullOrEmpty(memberId))
                {
                    return BadRequest("Member ID is required");
                }

                if (request == null || string.IsNullOrEmpty(request.Role))
                {
                    return BadRequest("Role is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid currentUserId))
                {
                    _logger.LogInformation("Updating role for member {MemberId} in project {ProjectId} to {Role} by user {UserId}",
                    memberId, projectId, request.Role, currentUserId);

                    var response = await _projectMemberGrpcClient.UpdateMemberRoleAsync(
                        projectId,
                        memberId,
                        request.Role.ToUpper(),
                        currentUserId.ToString());

                    if (response.Success)
                    {
                        return Ok(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for member {MemberId} in project {ProjectId}", memberId, projectId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    statusCode = 500
                });
            }
        }

        [HttpPut("applied-project")]
        public async Task<IActionResult> ApplyToProject(string projectId)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }
                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid currentUserId))
                {

                    var userEmail = User.FindFirstValue(ClaimTypes.Email);
                    var userName = User.FindFirstValue(ClaimTypes.Name);

                    _logger.LogInformation("User is applying to project {ProjectId}", projectId);
                    var response = await _projectMemberGrpcClient.UpdateApprovedStatusAsync(projectId, currentUserId.ToString(), userName, userEmail, ApprovedStatus.Pending.ToString(), currentUserId.ToString());
                    if (response.Success)
                    {
                        return Ok(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            success = response.Success,
                            message = response.Message,
                            statusCode = response.StatusCode
                        });
                    }
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying to project {ProjectId} by user ", projectId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    statusCode = 500
                });
            }
        }


    }
}