using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tasco.NotificationService.Core.Interfaces;
using Tasco.NotificationService.Core.Models;
using Tasco.NotificationService.Worker.Constants;

namespace Tasco.NotificationService.Worker.Consumers
{
    public class NotificationMessageConsumer : BackgroundService
    {
        private readonly ILogger<NotificationMessageConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRabbitMQConnection _rabbitMQConnection;
        private IConnection? _connection;
        private IChannel? _channel;

        public NotificationMessageConsumer(
            ILogger<NotificationMessageConsumer> logger,
            IServiceScopeFactory serviceScopeFactory,
            IRabbitMQConnection rabbitMQConnection)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _rabbitMQConnection = rabbitMQConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Retry logic for connecting to RabbitMQ
            var maxRetries = 10;
            var retryCount = 0;
            
            while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Attempting to connect to RabbitMQ (attempt {RetryCount}/{MaxRetries})", retryCount + 1, maxRetries);
                    
                    _connection = _rabbitMQConnection.GetConnection();
                    _channel = await _connection.CreateChannelAsync();

                    await _channel.QueueDeclareAsync(
                        queue: QueueNames.NotificationQueue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _logger.LogInformation("Successfully connected to RabbitMQ and declared queue");
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, "Failed to connect to RabbitMQ (attempt {RetryCount}/{MaxRetries})", retryCount, maxRetries);
                    
                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} attempts. Service will stop.", maxRetries);
                        return;
                    }
                    
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            if (_channel == null)
            {
                _logger.LogError("Channel is null, cannot proceed");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                _logger.LogInformation("🔔 MESSAGE RECEIVED - DeliveryTag: {DeliveryTag}", ea.DeliveryTag);
                
                using var scope = _serviceScopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    
                    _logger.LogInformation("📨 Raw message content: {MessageJson}", messageJson);

                    var notificationMessage = JsonConvert.DeserializeObject<NotificationMessage>(messageJson);
                    if (notificationMessage == null)
                    {
                        _logger.LogWarning("❌ Received null NotificationMessage");
                        return;
                    }

                    _logger.LogInformation("✅ Received notification: {Title}, Type: {Type}", notificationMessage.Title, notificationMessage.Type);

                    var result = await notificationService.SendNotificationAsync(notificationMessage);
                    _logger.LogInformation("📧 Notification sent: {Success}, Error: {Error}", result.IsSuccess, result.ErrorMessage ?? "None");

                    // Acknowledge message
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message from queue");
                    // You can implement retry or dead-letter logic here
                    // For now, we'll negative acknowledge to reject the message
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: QueueNames.NotificationQueue,
                autoAck: false, // Manual ack for reliability
                consumer: consumer);

            _logger.LogInformation("NotificationMessageConsumer started and listening for messages");

            // Keep the consumer running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async void Dispose()
        {
            if (_channel != null)
                await _channel.CloseAsync();
            if (_connection != null)
                await _connection.CloseAsync();
            base.Dispose();
        }
    }
}
