//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using Tasco.ProjectService.Service.BussinessModel.ProjectBussinessModel;
//using Tasco.ProjectService.Service.Services.Interface;

//namespace Tasco.ProjectService.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ProjectController : ControllerBase
//    {
//        private readonly IProjectService _projectService;

//        public ProjectController(IProjectService projectService)
//        {
//            _projectService = projectService;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetProjects([FromQuery] int page = 1, [FromQuery] int size = 10)
//        {
//            var result = await _projectService.GetProjectsAsync(page, size);

//            return result.Match<IActionResult>(
//                (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                (data, message) => Ok(new { Data = data, Message = message })
//            );
//        }

//        [HttpGet("{projectId:guid}")]
//        public async Task<IActionResult> GetProjectById(Guid projectId)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
//            {

//                var result = await _projectService.GetProjectByIdAsync(projectId);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }

//            return Unauthorized("Không thể xác định thông tin người dùng");
//        }

//        [HttpGet("my-project")]
//        public async Task<IActionResult> GetProjectsByMember([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? role = null, [FromQuery] string? search = null, [FromQuery] bool isDeleted = false)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
//            {
//                var result = await _projectService.GetProjectForMenber(userId, role, page, size, search, isDeleted);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }

//            return Unauthorized("Không thể xác định thông tin người dùng");
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
//            {
//                var result = await _projectService.CreateProjectAsync(request.Name, request.Description, userId);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }
//            return Unauthorized("Không thể xác định thông tin người dùng");
//        }

//        [HttpPut("{projectId:guid}")]
//        public async Task<IActionResult> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest request)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
//            {
//                var result = await _projectService.UpdateProjectAsync(projectId, request.Name, request.Description, userId);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }
//            return Unauthorized("Không thể xác định thông tin người dùng");

//        }

//        [HttpDelete("{projectId:guid}")]
//        public async Task<IActionResult> DeleteProject(Guid projectId)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId))
//            {
//                var result = await _projectService.DeleteProjectAsync(projectId, userId);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }
//            return Unauthorized("Không thể xác định thông tin người dùng");
//        }
//    }
//}