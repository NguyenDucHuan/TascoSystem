using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.TaskService.Repository.Entities
{
	public class Comment
	{
		[Key]
		public Guid Id { get; set; }
		public Guid TaskId { get; set; }
		public Guid UserId { get; set; }
		public string UserName { get; set; }
		public string Content { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; } = false;

		[ForeignKey("TaskId")]
		public virtual WorkTask WorkTask { get; set; }
	}
}
