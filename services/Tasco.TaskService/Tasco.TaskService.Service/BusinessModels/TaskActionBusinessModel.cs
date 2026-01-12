using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;

namespace Tasco.TaskService.Service.BusinessModels
{
    public class TaskActionBusinessModel
    {
        public Guid WorkTaskId { get; set; }
        [Required]
        [StringLength(50)]
        public string ActionType { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(1000)]
        public string OldValue { get; set; } = string.Empty;

        [StringLength(1000)]
        public string NewValue { get; set; } = string.Empty;

        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
