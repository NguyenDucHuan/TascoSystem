using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.TaskService.Service.BusinessModels
{
    public class WorkTaskBusinessModel
    {
        public Guid Id { get; set; }
        public Guid WorkAreaId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int Progress { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public List<TaskMemberBusinessModel> TaskMembers { get; set; } = new List<TaskMemberBusinessModel>();
        public List<TaskObjectiveBusinessModel> TaskObjectives { get; set; } = new List<TaskObjectiveBusinessModel>();
    }
}
