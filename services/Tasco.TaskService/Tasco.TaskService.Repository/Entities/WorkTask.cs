using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.TaskService.Repository.Entities
{
    public class WorkTask
    {
        [Key]
        public Guid Id { get; set; }

        public Guid WorkAreaId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent

        [StringLength(50)]
        public string Status { get; set; } = "Todo"; // Todo, InProgress, Review, Done

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        public int DisplayOrder { get; set; } = 0;

        [Range(0, 100)]
        public int Progress { get; set; } = 0; // Phần trăm hoàn thành

        // Thông tin người tạo task
        [Required]
        public Guid CreatedByUserId { get; set; }

        [StringLength(200)]
        public string CreatedByUserName { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Foreign key
        [ForeignKey("WorkAreaId")]
        public virtual WorkArea WorkArea { get; set; }

        // Navigation properties
        public virtual ICollection<TaskObjective> TaskObjectives { get; set; } = new List<TaskObjective>();
        public virtual ICollection<TaskMember> TaskMembers { get; set; } = new List<TaskMember>();
        public virtual ICollection<TaskAction> TaskActions { get; set; } = new List<TaskAction>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}