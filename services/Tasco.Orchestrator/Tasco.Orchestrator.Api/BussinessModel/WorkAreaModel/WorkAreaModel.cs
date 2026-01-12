using System.ComponentModel.DataAnnotations;

namespace Tasco.Orchestrator.Api.BussinessModel.WorkAreaModel
{
    /// <summary>
    /// DTO cho tạo work area
    /// </summary>
    public class CreateWorkAreaRequestDto
    {
        /// <summary>
        /// Tên work area
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả work area
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO cho cập nhật work area
    /// </summary>
    public class UpdateWorkAreaRequestDto
    {
        /// <summary>
        /// Tên work area
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả work area
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }
}
