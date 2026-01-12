using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.Shared.Notifications.Models;


namespace Tasco.ProjectService.Service.Services.Interface
{
    public interface INotificationService
    {
        Task SendNotificationAsync(NotificationMessage message);
        Task<bool> SendNotificationBatchAsync(IEnumerable<NotificationMessage> messages, CancellationToken cancellationToken = default);
    }
}
