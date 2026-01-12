namespace Tasco.Orchestrator.Api.BussinessModel.CommentModel
{
    /// <summary>
    /// DTO cho tạo comment
    /// </summary>
    public class CreateCommentRequest
    {
        /// <summary>
        /// ID của task
        /// </summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung comment
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho cập nhật comment
    /// </summary>
    public class UpdateCommentRequestDto
    {
        /// <summary>
        /// Nội dung comment mới
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
