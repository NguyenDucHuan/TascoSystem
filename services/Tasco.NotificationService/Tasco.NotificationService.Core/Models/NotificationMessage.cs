using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.NotificationService.Core.Models
{
    public class NotificationMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public string? ProjectId { get; set; }

        public string? TaskId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public Dictionary<string, object>? Metadata { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public List<NotificationChannel> Channels { get; set; } = new();
    }

    public enum NotificationType
    {
        TaskStatusChanged,
        TaskAssigned,
        TaskCommentAdded,
        ProjectCreated,
        ProjectUpdated,
        DeadlineReminder,
        MentionInComment
    }

    public enum NotificationPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    public enum NotificationChannel
    {
        InApp,
        Email,
        SMS,
        Push
    }
}
