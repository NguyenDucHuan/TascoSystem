using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.TaskService.Repository.Entities
{
    public class TaskAction
    {
        [Key]
        public Guid Id { get; set; }

        public Guid WorkTaskId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [StringLength(200)]
        public string UserName { get; set; }

        [Required]
        [StringLength(50)]
        public string ActionType { get; set; } // Created, Updated, Deleted, StatusChanged, MemberAdded, etc.

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(1000)]
        public string? OldValue { get; set; }

        [StringLength(1000)]
        public string? NewValue { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        // Foreign key
        [ForeignKey("WorkTaskId")]
        public virtual WorkTask WorkTask { get; set; }
    }
}