using System;
using System.ComponentModel.DataAnnotations;

namespace Tasco.TaskService.Service.BusinessModels
{
    public class CommentBusinessModel
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid TaskId { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [StringLength(200)]
        public string UserName { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsDeleted { get; set; }
    }
} 