using System.ComponentModel.DataAnnotations;

namespace Tasco.TaskService.API.Payload.Request
{
    public class WorkTaskRequest
    {
        public Guid WorkAreaId { get; set; }

        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
