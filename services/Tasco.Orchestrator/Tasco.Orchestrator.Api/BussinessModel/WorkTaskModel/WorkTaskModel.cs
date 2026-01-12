using System.ComponentModel.DataAnnotations;

namespace Tasco.Orchestrator.Api.BussinessModel.WorkTaskModel
{
    /// <summary>
    /// DTO cho tạo work task
    /// </summary>
    public class CreateWorkTaskRequestDto
    {
        /// <summary>
        /// Tiêu đề work task
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả work task
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái (NEW, IN_PROGRESS, COMPLETED, etc.)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Mức độ ưu tiên (LOW, MEDIUM, HIGH, URGENT)
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Ngày bắt đầu (yyyy-MM-dd)
        /// </summary>
        public string? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc (yyyy-MM-dd)
        /// </summary>
        public string? EndDate { get; set; }

        /// <summary>
        /// Ngày hạn (yyyy-MM-dd)
        /// </summary>
        public string? DueDate { get; set; }
    }

    /// <summary>
    /// DTO cho cập nhật work task
    /// </summary>
    public class UpdateWorkTaskRequestDto
    {
        /// <summary>
        /// Tiêu đề work task
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả work task
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái (NEW, IN_PROGRESS, COMPLETED, etc.)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Mức độ ưu tiên (LOW, MEDIUM, HIGH, URGENT)
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Ngày bắt đầu (yyyy-MM-dd)
        /// </summary>
        public string? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc (yyyy-MM-dd)
        /// </summary>
        public string? EndDate { get; set; }

        /// <summary>
        /// Ngày hạn (yyyy-MM-dd)
        /// </summary>
        public string? DueDate { get; set; }
    }
}
