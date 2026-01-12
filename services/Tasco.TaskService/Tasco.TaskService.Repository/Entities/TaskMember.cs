using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.TaskService.Repository.Entities
{
    public class TaskMember
    {
        [Key]
        public Guid Id { get; set; }

        public Guid WorkTaskId { get; set; }

        [Required]
        public Guid UserId { get; set; } // Id từ Identity service

        [StringLength(200)]
        public string UserName { get; set; } // Cache

        [StringLength(200)]
        public string? UserEmail { get; set; } // Nullable

        [StringLength(50)]
        public string Role { get; set; } = "Assignee"; // Assignee, Reviewer, Observer

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Required]
        public Guid AssignedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // Foreign key
        [ForeignKey("WorkTaskId")]
        public virtual WorkTask WorkTask { get; set; }
    }

}