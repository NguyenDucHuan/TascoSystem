using Microsoft.Extensions.Logging;
using Tasco.NotificationService.Core.Interfaces;
using Tasco.NotificationService.Core.Models;

namespace Tasco.NotificationService.Infrastructure.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private readonly ILogger<NotificationTemplateService> _logger;

        public NotificationTemplateService(ILogger<NotificationTemplateService> logger)
        {
            _logger = logger;
        }

        public async Task<TemplateResult> GenerateContentAsync(
            NotificationChannel channel, 
            NotificationType type, 
            NotificationMessage notification, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.CompletedTask; // Simulate async operation

                if (channel != NotificationChannel.Email)
                {
                    return new TemplateResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Template service doesn't support channel: {channel}"
                    };
                }

                var (subject, body) = GenerateEmailTemplate(type, notification);

                return new TemplateResult
                {
                    IsSuccess = true,
                    Subject = subject,
                    Body = body
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating template for notification {NotificationId}", notification.Id);
                return new TemplateResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private static (string Subject, string Body) GenerateEmailTemplate(NotificationType type, NotificationMessage notification)
        {
            var subject = type switch
            {
                NotificationType.TaskStatusChanged => $"📋 Trạng thái công việc đã thay đổi: {notification.Title}",
                NotificationType.TaskAssigned => $"✨ Bạn có công việc mới: {notification.Title}",
                NotificationType.TaskCommentAdded => $"💬 Bình luận mới: {notification.Title}",
                NotificationType.ProjectCreated => $"🎉 Dự án mới: {notification.Title}",
                NotificationType.ProjectUpdated => $"📝 Dự án được cập nhật: {notification.Title}",
                NotificationType.DeadlineReminder => $"⏰ Nhắc nhở deadline: {notification.Title}",
                NotificationType.MentionInComment => $"👋 Bạn được nhắc đến: {notification.Title}",
                _ => $"🔔 {notification.Title}"
            };

            var body = GenerateEmailBody(type, notification);
            return (subject, body);
        }

        private static string GenerateEmailBody(NotificationType type, NotificationMessage notification)
        {
            var priorityBadge = notification.Priority switch
            {
                NotificationPriority.Critical => "<span style='background: #ff4757; color: white; padding: 4px 8px; border-radius: 12px; font-size: 11px; font-weight: 600;'>🚨 KHẨN CẤP</span>",
                NotificationPriority.High => "<span style='background: #ff6348; color: white; padding: 4px 8px; border-radius: 12px; font-size: 11px; font-weight: 600;'>⚡ CAO</span>",
                NotificationPriority.Normal => "<span style='background: #3742fa; color: white; padding: 4px 8px; border-radius: 12px; font-size: 11px; font-weight: 600;'>📋 BÌNH THƯỜNG</span>",
                NotificationPriority.Low => "<span style='background: #57606f; color: white; padding: 4px 8px; border-radius: 12px; font-size: 11px; font-weight: 600;'>📝 THẤP</span>",
                _ => "<span style='background: #3742fa; color: white; padding: 4px 8px; border-radius: 12px; font-size: 11px; font-weight: 600;'>📋 BÌNH THƯỜNG</span>"
            };

            var greeting = GetFriendlyGreeting(type);
            var actionButton = GenerateActionButton(notification);
            var additionalInfo = GenerateSimpleAdditionalInfo(notification);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Thông báo từ Tasco</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f8fafc; line-height: 1.6;'>
    <div style='max-width: 600px; margin: 0 auto; background: white; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);'>
        
        <!-- Header với Logo -->
        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center;'>
            <img src='cid:tasco_logo' alt='Tasco Logo' style='width: 120px; height: auto; margin-bottom: 15px;' />
            <h1 style='color: white; margin: 0; font-size: 24px; font-weight: 300;'>Tasco System</h1>
            <p style='color: rgba(255,255,255,0.9); margin: 5px 0 0 0; font-size: 14px;'>Hệ thống quản lý công việc thông minh</p>
        </div>

        <!-- Nội dung chính -->
        <div style='padding: 40px 30px;'>
            
            <!-- Lời chào thân thiện -->
            <div style='margin-bottom: 25px;'>
                <h2 style='color: #2d3748; margin: 0 0 10px 0; font-size: 22px; font-weight: 600;'>
                    {greeting}
                </h2>
                {priorityBadge}
            </div>

            <!-- Tiêu đề thông báo -->
            <div style='background: #f7fafc; border-left: 4px solid #667eea; padding: 20px; margin: 25px 0; border-radius: 0 8px 8px 0;'>
                <h3 style='color: #2d3748; margin: 0 0 10px 0; font-size: 18px; font-weight: 600;'>{notification.Title}</h3>
                <p style='color: #4a5568; margin: 0; font-size: 16px; line-height: 1.5;'>{notification.Message}</p>
            </div>

            {additionalInfo}
            {actionButton}

            <!-- Thông tin chi tiết đơn giản -->
            <div style='background: #f8f9fa; border-radius: 8px; padding: 20px; margin: 25px 0;'>
                <h4 style='color: #4a5568; margin: 0 0 15px 0; font-size: 14px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;'>Chi tiết thông báo</h4>
                <table style='width: 100%; font-size: 14px;'>
                    <tr style='border-bottom: 1px solid #e2e8f0;'>
                        <td style='padding: 8px 0; color: #718096; font-weight: 500;'>Loại:</td>
                        <td style='padding: 8px 0; color: #2d3748;'>{GetFriendlyTypeName(type)}</td>
                    </tr>
                    <tr style='border-bottom: 1px solid #e2e8f0;'>
                        <td style='padding: 8px 0; color: #718096; font-weight: 500;'>Thời gian:</td>
                        <td style='padding: 8px 0; color: #2d3748;'>{notification.CreatedAt:dd/MM/yyyy HH:mm}</td>
                    </tr>
                    <tr>
                        <td style='padding: 8px 0; color: #718096; font-weight: 500;'>ID:</td>
                        <td style='padding: 8px 0; color: #718096; font-family: monospace; font-size: 12px;'>{notification.Id}</td>
                    </tr>
                </table>
            </div>
        </div>

        <!-- Footer thân thiện -->
        <div style='background: #f8f9fa; padding: 25px; text-align: center; border-top: 1px solid #e2e8f0;'>
            <p style='color: #718096; margin: 0 0 10px 0; font-size: 14px;'>
                💙 Cảm ơn bạn đã sử dụng <strong>Tasco System</strong>
            </p>
            <p style='color: #a0aec0; margin: 0; font-size: 12px;'>
                Đây là email tự động. Nếu có thắc mắc, vui lòng liên hệ quản trị viên.
            </p>
        </div>
    </div>
</body>
</html>";
        }

        private static string GetFriendlyGreeting(NotificationType type)
        {
            return type switch
            {
                NotificationType.TaskStatusChanged => "👋 Xin chào! Có cập nhật về công việc của bạn",
                NotificationType.TaskAssigned => "🎯 Bạn có một nhiệm vụ mới!",
                NotificationType.TaskCommentAdded => "💬 Có người vừa bình luận",
                NotificationType.ProjectCreated => "🚀 Dự án mới đã được tạo",
                NotificationType.ProjectUpdated => "📝 Dự án có cập nhật mới",
                NotificationType.DeadlineReminder => "⏰ Nhắc nhở quan trọng!",
                NotificationType.MentionInComment => "👤 Bạn được nhắc đến",
                _ => "🔔 Bạn có thông báo mới"
            };
        }

        private static string GetFriendlyTypeName(NotificationType type)
        {
            return type switch
            {
                NotificationType.TaskStatusChanged => "Cập nhật công việc",
                NotificationType.TaskAssigned => "Giao việc mới",
                NotificationType.TaskCommentAdded => "Bình luận mới",
                NotificationType.ProjectCreated => "Tạo dự án",
                NotificationType.ProjectUpdated => "Cập nhật dự án",
                NotificationType.DeadlineReminder => "Nhắc nhở deadline",
                NotificationType.MentionInComment => "Được nhắc đến",
                _ => type.ToString()
            };
        }

        private static string GenerateActionButton(NotificationMessage notification)
        {
            var actionUrl = GenerateActionUrl(notification);
            
            if (string.IsNullOrWhiteSpace(actionUrl))
                return "";

            return $@"
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{actionUrl}' 
                   style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                          color: white; 
                          text-decoration: none; 
                          padding: 12px 30px; 
                          border-radius: 25px; 
                          display: inline-block; 
                          font-weight: 600; 
                          font-size: 14px;
                          box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
                          transition: all 0.3s ease;'>
                    🔍 Xem chi tiết
                </a>
            </div>";
        }

        private static string GenerateActionUrl(NotificationMessage notification)
        {
            var baseUrl = "https://tasco.app"; // This should come from configuration
            
            return notification.Type switch
            {
                NotificationType.TaskStatusChanged or NotificationType.TaskAssigned or NotificationType.TaskCommentAdded 
                    when !string.IsNullOrWhiteSpace(notification.TaskId) 
                    => $"{baseUrl}/tasks/{notification.TaskId}",
                
                NotificationType.ProjectCreated or NotificationType.ProjectUpdated 
                    when !string.IsNullOrWhiteSpace(notification.ProjectId) 
                    => $"{baseUrl}/projects/{notification.ProjectId}",
                
                _ => $"{baseUrl}/notifications"
            };
        }

        private static string GenerateSimpleAdditionalInfo(NotificationMessage notification)
        {
            var info = new List<string>();

            if (!string.IsNullOrWhiteSpace(notification.ProjectId))
            {
                info.Add($"📁 <strong>Dự án:</strong> {notification.ProjectId}");
            }

            if (!string.IsNullOrWhiteSpace(notification.TaskId))
            {
                info.Add($"📋 <strong>Công việc:</strong> {notification.TaskId}");
            }

            if (notification.Metadata != null && notification.Metadata.Any())
            {
                foreach (var metadata in notification.Metadata.Take(2))
                {
                    if (metadata.Key != "email" && metadata.Key != "userEmail")
                    {
                        var friendlyKey = GetFriendlyMetadataKey(metadata.Key);
                        info.Add($"ℹ️ <strong>{friendlyKey}:</strong> {metadata.Value}");
                    }
                }
            }

            if (!info.Any())
                return "";

            return $@"
            <div style='background: #e6fffa; border: 1px solid #81e6d9; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                <h4 style='color: #234e52; margin: 0 0 10px 0; font-size: 14px; font-weight: 600;'>📝 Thông tin bổ sung</h4>
                <div style='color: #234e52; font-size: 14px; line-height: 1.6;'>
                    {string.Join("<br>", info)}
                </div>
            </div>";
        }

        private static string GetFriendlyMetadataKey(string key)
        {
            return key.ToLower() switch
            {
                "assignee" => "Người được giao",
                "reporter" => "Người báo cáo",
                "status" => "Trạng thái",
                "priority" => "Độ ưu tiên",
                "deadline" => "Hạn chót",
                "comment" => "Bình luận",
                "author" => "Tác giả",
                _ => key
            };
        }
    }
}
