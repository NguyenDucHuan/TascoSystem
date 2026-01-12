using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.ProjectService.Service.BussinessModel.ProjectBussinessModel
{
    public class ProjectResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public Guid UpdateBy { get; set; } = Guid.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<ProjectMemberResponse> Members { get; set; } = new List<ProjectMemberResponse>();
    }
    public class ProjectMemberResponse
    {
        public Guid UserId { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string ApprovedStatus { get; set; }
        public DateTime ApprovedUpdateDate { get; set; }
        public bool IsRemoved { get; set; } = false;
        public DateTime? RemoveDate { get; set; }

    }
}
