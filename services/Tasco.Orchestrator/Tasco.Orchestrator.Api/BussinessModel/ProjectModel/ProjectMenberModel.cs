namespace Tasco.Orchestrator.Api.BussinessModel.ProjectModel
{
    public class ProjectMenberModel
    {
        /// <summary>
        /// DTO cho cập nhật trạng thái phê duyệt
        /// </summary>
        public class UpdateApprovedStatusRequest
        {
            /// <summary>
            /// Trạng thái phê duyệt: PENDING, APPROVED, REJECTED, REMOVED
            /// </summary>
            public string ApprovedStatus { get; set; } = string.Empty;
        }

        /// <summary>
        /// DTO cho cập nhật vai trò thành viên
        /// </summary>
        public class UpdateMemberRoleRequest
        {
            /// <summary>
            /// Vai trò: MEMBER, OWNER
            /// </summary>
            public string Role { get; set; } = string.Empty;
        }
    }
}
