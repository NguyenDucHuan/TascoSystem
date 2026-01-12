using Microsoft.Extensions.Logging;
using Tasco.NotificationService.Core.Interfaces;
using Tasco.NotificationService.Core.Models;

namespace Tasco.NotificationService.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IEnumerable<INotificationChannel> _channels;

        public NotificationService(
            ILogger<NotificationService> logger,
            IEnumerable<INotificationChannel> channels)
        {
            _logger = logger;
            _channels = channels;
        }

        public async Task<NotificationResult> SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        {
            var result = new NotificationResult 
            { 
                IsSuccess = false,
                NotificationId = notification.Id
            };

            try
            {
                if (notification.Channels == null || !notification.Channels.Any())
                {
                    _logger.LogWarning("No notification channels specified for notification {NotificationId}", notification.Id);
                    result.ErrorMessage = "No notification channels specified";
                    return result;
                }

                var channelResults = new Dictionary<NotificationChannel, ChannelResult>();
                var hasAnySuccess = false;

                // Process each channel
                foreach (var requestedChannel in notification.Channels)
                {
                    var channel = _channels.FirstOrDefault(c => c.CanHandle(requestedChannel));
                    if (channel == null)
                    {
                        _logger.LogWarning("No notification channel handler found for channel: {Channel} in notification {NotificationId}", 
                            requestedChannel, notification.Id);
                        
                        channelResults[requestedChannel] = new ChannelResult
                        {
                            IsSuccess = false,
                            ErrorMessage = $"No handler found for channel: {requestedChannel}",
                            SentAt = DateTime.UtcNow
                        };
                        continue;
                    }

                    try
                    {
                        var channelResult = await channel.SendAsync(notification, cancellationToken);
                        channelResults[requestedChannel] = channelResult;
                        
                        if (channelResult.IsSuccess)
                        {
                            hasAnySuccess = true;
                            _logger.LogInformation("Successfully sent notification {NotificationId} via {Channel}", 
                                notification.Id, requestedChannel);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to send notification {NotificationId} via {Channel}: {Error}", 
                                notification.Id, requestedChannel, channelResult.ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception occurred while sending notification {NotificationId} via {Channel}", 
                            notification.Id, requestedChannel);
                        
                        channelResults[requestedChannel] = new ChannelResult
                        {
                            IsSuccess = false,
                            ErrorMessage = ex.Message,
                            SentAt = DateTime.UtcNow
                        };
                    }
                }

                result.ChannelResults = channelResults;
                result.IsSuccess = hasAnySuccess;
                
                if (!hasAnySuccess)
                {
                    result.ErrorMessage = "Failed to send notification via any channel";
                }

                _logger.LogInformation("Notification {NotificationId} processing completed. Success: {Success}, Channels processed: {ChannelCount}", 
                    notification.Id, result.IsSuccess, channelResults.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification {NotificationId}", notification.Id);
                result.ErrorMessage = ex.Message;
            }

            return result;
        }
    }
} 