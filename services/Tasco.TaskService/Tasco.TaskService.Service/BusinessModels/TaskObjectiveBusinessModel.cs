using System;
using System.ComponentModel.DataAnnotations;

namespace Tasco.TaskService.Service.BusinessModels
{
    public class TaskObjectiveBusinessModel
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid WorkTaskId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        public int DisplayOrder { get; set; }
        
        [Required]
        public Guid CreatedByUserId { get; set; }
        
        public Guid? CompletedByUserId { get; set; }
        
        public bool IsDeleted { get; set; }
    }
} 