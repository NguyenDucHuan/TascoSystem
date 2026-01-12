using System;
using System.Collections.Generic;

namespace Tasco.TaskService.Service.BusinessModels
{
    public class TaskMemberBusinessModel
    {
        public Guid Id { get; set; }
        public Guid WorkTaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime AssignedDate { get; set; }
        public Guid AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; }
        public DateTime? RemovedDate { get; set; }
        public Guid? RemovedByUserId { get; set; }
        public string RemovedByUserName { get; set; }
    }
} 