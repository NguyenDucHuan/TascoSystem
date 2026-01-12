namespace Tasco.Orchestrator.Api.BussinessModel.SubTaskModel
{

    /// <summary>
    /// DTO cho tạo subtask
    /// </summary>
    public class CreateSubTaskRequest
    {
        /// <summary>
        /// Tiêu đề subtask
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả subtask
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO cho cập nhật subtask
    /// </summary>
    public class UpdateSubTaskRequestDto
    {
        /// <summary>
        /// Tiêu đề subtask
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả subtask
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        ///     status
        /// </summary>
        public string? Status { get; set; }

    }

    /// <summary>
    /// DTO cho hoàn thành subtask
    /// </summary>
    public class CompleteSubTaskRequestDto
    {
        /// <summary>
        /// Trạng thái hoàn thành
        /// </summary>
        public bool IsCompleted { get; set; }
    }

    /// <summary>
    /// DTO cho gán subtask
    /// </summary>
    public class AssignSubTaskRequestDto
    {
        /// <summary>
        /// ID người được gán
        /// </summary>
        public string AssigneeId { get; set; } = string.Empty;

        /// <summary>
        /// Tên người được gán
        /// </summary>
        public string? AssigneeName { get; set; }
    }

}
