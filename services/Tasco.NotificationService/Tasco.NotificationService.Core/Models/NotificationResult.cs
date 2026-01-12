using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.NotificationService.Core.Models;

namespace Tasco.NotificationService.Core.Models
{
    public class NotificationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string NotificationId { get; set; } = string.Empty;
        public Dictionary<NotificationChannel, ChannelResult> ChannelResults { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChannelResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ExternalId { get; set; } // ID từ external service (email service, SMS service, etc.)
        public DateTime SentAt { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
