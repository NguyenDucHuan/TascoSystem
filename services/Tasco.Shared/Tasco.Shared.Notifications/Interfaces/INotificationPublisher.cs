using Tasco.Shared.Notifications.Models;

namespace Tasco.Shared.Notifications.Interfaces
{
    public interface INotificationPublisher
    {
        Task<bool> PublishAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
        Task<bool> PublishBatchAsync(IEnumerable<NotificationMessage> notifications, CancellationToken cancellationToken = default);
    }
} 