using System.ComponentModel.DataAnnotations;

namespace Tasco.TaskService.API.Payload.Request
{
    public class WorkAreaRequest
    {
        public Guid ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
