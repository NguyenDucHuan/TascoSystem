using System.ComponentModel.DataAnnotations;

namespace Tasco.Orchestrator.Api.BussinessModel.TaskMenberController
{
    /// <summary>
    /// DTO cho tạo task member
    /// </summary>
    public class CreateTaskMemberRequestDto
    {
        /// <summary>
        /// ID người dùng
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Tên người dùng
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Email người dùng
        /// </summary>
        public string? UserEmail { get; set; }

        /// <summary>
        /// Vai trò trong task (MEMBER, LEADER, etc.)
        /// </summary>
        public string? Role { get; set; }
    }

    /// <summary>
    /// DTO cho cập nhật task member
    /// </summary>
    public class UpdateTaskMemberRequestDto
    {
        /// <summary>
        /// ID người dùng
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Tên người dùng
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Email người dùng
        /// </summary>
        public string? UserEmail { get; set; }

        /// <summary>
        /// Vai trò trong task
        /// </summary>
        public string? Role { get; set; }
    }
}
