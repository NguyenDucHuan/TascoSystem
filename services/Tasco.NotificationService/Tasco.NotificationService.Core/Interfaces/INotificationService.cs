using Tasco.NotificationService.Core.Models;

namespace Tasco.NotificationService.Core.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResult> SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
    }
} 