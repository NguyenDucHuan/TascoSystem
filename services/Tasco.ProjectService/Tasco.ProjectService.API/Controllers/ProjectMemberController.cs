//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using Tasco.ProjectService.Service.BussinessModel.ProjectBussinessModel;
//using Tasco.ProjectService.Service.Constant;
//using Tasco.ProjectService.Service.Services.Interface;

//namespace Tasco.ProjectService.API.Controllers
//{
//    [ApiController]
//    [Route("api/projects/{projectId:guid}/members")]
//    public class ProjectMemberController : ControllerBase
//    {
//        private readonly IProjectMemberService _projectMemberService;

//        public ProjectMemberController(IProjectMemberService projectMemberService)
//        {
//            _projectMemberService = projectMemberService;
//        }

//        /// <summary>
//        /// Xóa thành viên khỏi dự án
//        /// </summary>
//        /// <param name="projectId">ID của dự án</param>
//        /// <param name="memberId">ID của thành viên cần xóa</param>
//        /// <returns>Kết quả xóa thành viên</returns>
//        [HttpDelete("{memberId:guid}")]
//        public async Task<IActionResult> RemoveMemberFromProject(Guid projectId, Guid memberId)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid ownerId))
//            {
//                var result = await _projectMemberService.RemoveMemberFromProjectAsync(projectId, memberId, ownerId);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }

//            return Unauthorized("Không thể xác định thông tin người dùng");
//        }

//        /// <summary>
//        /// Cập nhật trạng thái phê duyệt của thành viên
//        /// </summary>
//        /// <param name="projectId">ID của dự án</param>
//        /// <param name="memberId">ID của thành viên</param>
//        /// <param name="request">Trạng thái phê duyệt mới</param>
//        /// <returns>Kết quả cập nhật</returns>
//        [HttpPut("{memberId:guid}/status")]
//        public async Task<IActionResult> UpdateApprovedStatus(Guid projectId, Guid memberId, [FromBody] UpdateApprovedStatusRequest request)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid ownerId))
//            {
//                var result = await _projectMemberService.UpdateApprovedStatusAsync(projectId, memberId, request.ApprovedStatus, ownerId);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }

//            return Unauthorized("Không thể xác định thông tin người dùng");
//        }

//        /// <summary>
//        /// Cập nhật vai trò của thành viên
//        /// </summary>
//        /// <param name="projectId">ID của dự án</param>
//        /// <param name="memberId">ID của thành viên</param>
//        /// <param name="request">Vai trò mới</param>
//        /// <returns>Kết quả cập nhật</returns>
//        [HttpPut("{memberId:guid}/role")]
//        public async Task<IActionResult> UpdateMemberRole(Guid projectId, Guid memberId, [FromBody] UpdateMemberRoleRequest request)
//        {
//            if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid ownerId))
//            {
//                var result = await _projectMemberService.UpdateMemberRoleAsync(projectId, memberId, request.Role, ownerId);

//                return result.Match<IActionResult>(
//                    (errorMessage, statusCode) => StatusCode(statusCode, errorMessage),
//                    (data, message) => Ok(new { Data = data, Message = message })
//                );
//            }

//            return Unauthorized("Không thể xác định thông tin người dùng");
//        }
//    }

//    /// <summary>
//    /// Request model cho cập nhật trạng thái phê duyệt
//    /// </summary>
//    public class UpdateApprovedStatusRequest
//    {
//        /// <summary>
//        /// Trạng thái phê duyệt: PENDING, APPROVED, REJECTED, REMOVED
//        /// </summary>
//        public string ApprovedStatus { get; set; } = string.Empty;
//    }

//    /// <summary>
//    /// Request model cho cập nhật vai trò thành viên
//    /// </summary>
//    public class UpdateMemberRoleRequest
//    {
//        /// <summary>
//        /// Vai trò: MEMBER, OWNER
//        /// </summary>
//        public string Role { get; set; } = string.Empty;
//    }
//}