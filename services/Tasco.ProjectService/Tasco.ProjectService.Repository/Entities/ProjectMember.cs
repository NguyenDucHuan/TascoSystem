using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.ProjectService.Repository.Entities
{
    public class ProjectMember
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string Role { get; set; } = "MEMBER";
        public string Status { get; set; } = "PENDING";
        public DateTime ApprovedUpdateDate { get; set; } = DateTime.UtcNow;
        public Guid? ApprovedBy { get; set; }
        public DateTime? RemoveDate { get; set; }
        public Guid? RemovedBy { get; set; }

        public Project Project { get; set; } = null!;
    }
}
