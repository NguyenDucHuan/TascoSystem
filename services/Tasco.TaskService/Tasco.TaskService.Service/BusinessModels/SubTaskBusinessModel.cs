using System;
using System.ComponentModel.DataAnnotations;

namespace Tasco.TaskService.Service.BusinessModels
{
    public class SubTaskBusinessModel
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid ParentTaskId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Title { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        public Guid? AssigneeId { get; set; }
        
        [StringLength(200)]
        public string AssigneeName { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        public bool IsDeleted { get; set; }
    }
} 