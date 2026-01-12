using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using Tasco.Shared.Notifications.Interfaces;
using Tasco.Shared.Notifications.Models;

namespace Tasco.Shared.Notifications.Publishers
{
    public class RabbitMQNotificationPublisher : INotificationPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel; // Sử dụng IChannel cho các phương thức bất đồng bộ
        private const string QueueName = "notification_queue"; // Tên queue cố định

        public RabbitMQNotificationPublisher()
        {
            // 🔧 Cấu hình kết nối RabbitMQ
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "admin",
                Password = "admin123",
                VirtualHost = "/",
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            };

            // 🔄 Thực hiện kết nối và tạo kênh với logic retry
            // Gọi các phương thức bất đồng bộ và chặn luồng bằng .Result trong constructor
            // (Như đã thảo luận, trong ứng dụng thực tế, cân nhắc một phương thức khởi tạo Async riêng hoặc DI)
            _connection = ConnectRabbitMQWithRetries(factory).Result;
            _channel = _connection.CreateChannelAsync().Result; // Sử dụng CreateChannelAsync().Result

            // Khai báo queue bất đồng bộ
            _channel.QueueDeclareAsync(queue: QueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null).Wait(); // Chặn để đảm bảo queue được khai báo trước khi tiếp tục

            Console.WriteLine($"✅ Connected to RabbitMQ and queue '{QueueName}' declared successfully.");
        }

        private async Task<IConnection> ConnectRabbitMQWithRetries(ConnectionFactory factory)
        {
            const int maxRetries = 5;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"🔄 Attempt {attempt}/{maxRetries}: Connecting to RabbitMQ...");
                    Console.WriteLine($"   Host: {factory.HostName}:{factory.Port}");
                    Console.WriteLine($"   User: {factory.UserName}");

                    var connection = await factory.CreateConnectionAsync();
                    Console.WriteLine("✅ Connected to RabbitMQ successfully!");
                    return connection;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Attempt {attempt} failed: {ex.Message}");
                    if (attempt == maxRetries)
                    {
                        Console.WriteLine("\n💡 Troubleshooting suggestions:");
                        Console.WriteLine("1. Make sure Docker containers are running: `docker-compose ps`");
                        Console.WriteLine("2. Check RabbitMQ container logs: `docker-compose logs rabbitmq`");
                        Console.WriteLine("3. Verify RabbitMQ Management UI: http://localhost:15672 (admin/admin123)");
                        Console.WriteLine("4. Check if notification-service is consuming: `docker-compose logs notification-service`");
                        throw new Exception("Failed to connect to RabbitMQ after multiple retries.", ex);
                    }
                    Console.WriteLine($"⏳ Waiting 3 seconds before retry...\n");
                    await Task.Delay(3000);
                }
            }
            return null; // Sẽ không bao giờ đạt được do throw exception
        }

        public async Task<bool> PublishAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested(); // Kiểm tra yêu cầu hủy bỏ

                var json = JsonConvert.SerializeObject(notification, Formatting.Indented);
                var body = Encoding.UTF8.GetBytes(json);

                // Khởi tạo BasicProperties trực tiếp như trong code mẫu của bạn
                // Điều này giải quyết cả 2 lỗi bạn đã gặp
                var properties = new BasicProperties
                {
                    Persistent = true, // Tin nhắn sẽ không bị mất khi RabbitMQ restart
                    MessageId = notification.Id,
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                    ContentType = "application/json"
                };

                // Sử dụng BasicPublishAsync với đối tượng BasicProperties đã khởi tạo
                await _channel.BasicPublishAsync(
                    exchange: "", // Exchange mặc định
                    routingKey: QueueName,
                    mandatory: false,
                    basicProperties: properties, // Truyền đối tượng BasicProperties
                    body: body);

                Console.WriteLine($"✅ Message sent: {notification.Type}");
                Console.WriteLine($"   📧 To: {notification.UserId}");
                Console.WriteLine($"   🏷️  Priority: {notification.Priority}");
                Console.WriteLine($"   📝 Title: {notification.Title}");
                Console.WriteLine("─────────────────────────────────────");
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"❌ Publish cancelled for message ID: {notification.Id}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send message (ID: {notification.Id}): {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PublishBatchAsync(IEnumerable<NotificationMessage> notifications, CancellationToken cancellationToken = default)
        {
            var success = true;
            foreach (var notification in notifications)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("⚠️ Batch publish cancelled.");
                    success = false;
                    break;
                }
                var published = await PublishAsync(notification, cancellationToken);
                if (!published)
                {
                    success = false; // Đánh dấu là có lỗi nếu một tin nhắn không gửi được
                }
                // Thêm độ trễ nhỏ để tránh quá tải RabbitMQ khi gửi số lượng lớn tin nhắn liên tục
                await Task.Delay(100, cancellationToken);
            }
            return success;
        }

        public void Dispose()
        {
            // Đóng kênh và kết nối RabbitMQ một cách an toàn
            // Gọi các phương thức CloseAsync() và chặn bằng .Wait() trong Dispose
            try
            {
                _channel?.CloseAsync().Wait(); // Chặn và chờ CloseAsync hoàn thành
                _channel?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error closing channel: {ex.Message}");
            }

            try
            {
                _connection?.CloseAsync().Wait(); // Chặn và chờ CloseAsync hoàn thành
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error closing connection: {ex.Message}");
            }

            Console.WriteLine("🗑️ RabbitMQ connection and channel disposed.");
            GC.SuppressFinalize(this); // Ngăn không cho Finalize được gọi
        }
    }
}