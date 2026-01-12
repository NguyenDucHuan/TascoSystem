namespace Tasco.Orchestrator.Api.BussinessModel.ProjectModel
{
    public class ProjectRequest
    {/// <summary>
     /// DTO cho tạo dự án mới
     /// </summary>
        public class CreateProjectRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// DTO cho cập nhật dự án
        /// </summary>
        public class UpdateProjectRequest
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
        }
    }
}
