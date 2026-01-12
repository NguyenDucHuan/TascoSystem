using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.TaskService.Repository.Entities
{
    public class TaskObjective
    {
        [Key]
        public Guid Id { get; set; }

        public Guid WorkTaskId { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [Required]
        public Guid CreatedByUserId { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Foreign key
        [ForeignKey("WorkTaskId")]
        public virtual WorkTask WorkTask { get; set; }
    }
}