using Tasco.NotificationService.Core.Models;

namespace Tasco.NotificationService.Core.Interfaces
{
    public interface INotificationTemplateService
    {
        Task<TemplateResult> GenerateContentAsync(
            NotificationChannel channel, 
            NotificationType type, 
            NotificationMessage notification, 
            CancellationToken cancellationToken = default);
    }

    public class TemplateResult
    {
        public bool IsSuccess { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
} 