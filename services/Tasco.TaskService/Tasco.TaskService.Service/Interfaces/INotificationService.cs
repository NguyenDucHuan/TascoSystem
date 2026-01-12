using System.Threading.Tasks;
using Tasco.TaskService.Repository.Entities;
using System.Collections.Generic;
using System.Threading;
using Tasco.Shared.Notifications.Models;

namespace Tasco.TaskService.Service.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationMessage message);
        Task<bool> SendNotificationBatchAsync(IEnumerable<NotificationMessage> messages, CancellationToken cancellationToken = default);
    }
}