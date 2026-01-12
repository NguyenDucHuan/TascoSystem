using Tasco.TaskService.Repository.Entities;
using Tasco.TaskService.Service.Interfaces;
using Tasco.Shared.Notifications.Interfaces;
using Tasco.Shared.Notifications.Models;

namespace Tasco.TaskService.Service.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationPublisher _notificationPublisher;

        public NotificationService(INotificationPublisher notificationPublisher)
        {
            _notificationPublisher = notificationPublisher;
        }

        public async Task SendNotificationAsync(NotificationMessage message)
        {
            var sharedNotificationMessage = new NotificationMessage
            {
                Id = message.Id,
                UserId = message.UserId,
                Title = message.Title,
                Message = message.Message,
                Type = message.Type,
                ProjectId = message.ProjectId,
                TaskId = message.TaskId,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead,
                Metadata = message.Metadata,
                Priority = message.Priority,
                Channels = message.Channels.Select(c => c).ToList()
            };

            await _notificationPublisher.PublishAsync(sharedNotificationMessage);
        }

        public async Task<bool> SendNotificationBatchAsync(IEnumerable<NotificationMessage> messages, CancellationToken cancellationToken = default)
        {
            var sharedNotificationMessages = messages.Select(message => new NotificationMessage
            {
                Id = message.Id,
                UserId = message.UserId,
                Title = message.Title,
                Message = message.Message,
                Type = message.Type,
                ProjectId = message.ProjectId,
                TaskId = message.TaskId,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead,
                Metadata = message.Metadata,
                Priority = message.Priority,
                Channels = message.Channels.Select(c => c).ToList()
            });

            return await _notificationPublisher.PublishBatchAsync(sharedNotificationMessages, cancellationToken);
        }
    }
} 