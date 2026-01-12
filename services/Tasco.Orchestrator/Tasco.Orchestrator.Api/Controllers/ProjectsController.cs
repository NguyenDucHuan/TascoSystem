using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tasco.Orchestrator.Infrastructure.GrpcClients;
using Tasco.Orchestrator.Api.BussinessModel.ProjectModel;
using static Tasco.Orchestrator.Api.BussinessModel.ProjectModel.ProjectRequest;
using System.Data;

namespace Tasco.Orchestrator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectGrpcClientService _projectGrpcClient;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            ProjectGrpcClientService projectGrpcClient,
            ILogger<ProjectsController> logger)
        {
            _projectGrpcClient = projectGrpcClient;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin dự án theo ID
        /// </summary>
        /// <param name="projectId">ID của dự án</param>
        /// <returns>Thông tin chi tiết dự án</returns>
        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectById(string projectId)
        {
            try
            {
                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting project {ProjectId} for user {UserId}", projectId, userId);

                    var response = await _projectGrpcClient.GetProjectByIdAsync(projectId, userId.ToString());

                    return Ok(response);
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project {ProjectId}", projectId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Tạo dự án mới
        /// </summary>
        /// <param name="request">Thông tin dự án mới</param>
        /// <returns>Kết quả tạo dự án</returns>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {

                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest("Project name is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Creating project {Name} for user {UserId}", request.Name, userId);

                    var ownerName = User.FindFirstValue(ClaimTypes.Name);
                    var ownerEmail = User.FindFirstValue(ClaimTypes.Email);

                    var response = await _projectGrpcClient.CreateProjectAsync(
                        request.Name,
                        request.Description ?? "",
                        userId.ToString(),
                        ownerName,
                        ownerEmail);

                    if (response.Data)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest(response.Message);
                    }
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project {Name}", request?.Name);
                return StatusCode(500, "Internal server error");
            }
        }




        /// <summary>
        /// Cập nhật dự án
        /// </summary>
        /// <param name="projectId">ID dự án cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(string projectId, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }

                if (request == null)
                {
                    return BadRequest("Request body is required");
                }

                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest("Project name is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {

                    _logger.LogInformation("Updating project {ProjectId} by user {UserId}", projectId, userId);

                    var response = await _projectGrpcClient.UpdateProjectAsync(
                        projectId,
                        request.Name,
                        request.Description ?? "",
                        userId.ToString());

                    if (response.Data)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest(response.Message);
                    }
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", projectId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Xóa dự án
        /// </summary>
        /// <param name="projectId">ID dự án cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return BadRequest("Project ID is required");
                }

                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {

                    _logger.LogInformation("Deleting project {ProjectId} by user {UserId}", projectId, userId);

                    var response = await _projectGrpcClient.DeleteProjectAsync(projectId, userId.ToString());

                    if (response.Data)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        return BadRequest(response.Message);
                    }
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", projectId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy danh sách dự án cho thành viên
        /// </summary>
        /// <param name="role">Vai trò của thành viên</param>
        /// <param name="pageSize">Số lượng item trên mỗi trang</param>
        /// <param name="pageNumber">Số trang hiện tại</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <param name="isDelete">Có lấy dự án đã xóa không</param>
        /// <returns>Danh sách dự án có phân trang</returns>
        [HttpGet("my-project")]
        public async Task<IActionResult> GetProjectsForMember(
            [FromQuery] string role = "",
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            [FromQuery] string search = "",
            [FromQuery] bool isDelete = false)
        {
            try
            {
                if (pageSize <= 0 || pageSize > 1000)
                {
                    pageSize = 10;
                }

                if (pageNumber <= 0)
                {
                    pageNumber = 1;
                }
                if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
                {
                    _logger.LogInformation("Getting projects for user {UserId}, role {Role}, page {PageNumber}",
                    userId, role, pageNumber);

                    var response = await _projectGrpcClient.GetProjectForMemberAsync(
                        userId.ToString(),
                        role,
                        pageSize,
                        pageNumber,
                        search,
                        isDelete);

                    return Ok(response);
                }
                return Unauthorized("Không thể xác định thông tin người dùng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects for member");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lấy danh sách dự án để apply
        /// </summary>
        /// <param name="pageSize">Số lượng item trên mỗi trang</param>
        /// <param name="pageNumber">Số trang hiện tại</param>
        /// <param name="searchTearm">Từ khóa tìm kiếm</param>
        /// <param name="isDelete">Có lấy dự án đã xóa không nên để false hoặc null để tránh người dùng vô nhưng dự án bị xóa</param>
        /// <returns>Danh sách dự án có phân trang</returns>
        [HttpGet]
        public async Task<IActionResult> GetProjetcs(
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            [FromQuery] string search = "",
            [FromQuery] bool isDelete = false)
        {
            try
            {

                if (pageSize <= 0 || pageSize > 1000)
                {
                    pageSize = 10;
                }

                if (pageNumber <= 0)
                {
                    pageNumber = 1;
                }

                _logger.LogInformation("Getting projects for page {PageNumber}",
                   pageNumber);

                var response = await _projectGrpcClient.GetProjectForApplyAsync(
                    pageSize,
                    pageNumber,
                    search,
                    isDelete);
                return Ok(response);
            }
            catch
            {
                _logger.LogError("Error getting projects");
                return StatusCode(500, "Internal server error");
            }

        }

    }


}
