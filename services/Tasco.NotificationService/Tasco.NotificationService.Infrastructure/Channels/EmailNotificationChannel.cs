using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net.Mime;
using Tasco.NotificationService.Infrastructure.SMTPs.Repositories;
using Tasco.NotificationService.Core.Interfaces;
using Tasco.NotificationService.Core.Models;

namespace Tasco.NotificationService.Infrastructure.Channels
{
    public class EmailNotificationChannel : IEmailNotificationChannel
    {
        private readonly ILogger<EmailNotificationChannel> _logger;
        private readonly INotificationTemplateService _templateService;
        private readonly IEmailRepository _emailRepository;

        public EmailNotificationChannel(
            ILogger<EmailNotificationChannel> logger,
            INotificationTemplateService templateService,
            IEmailRepository emailRepository)
        {
            _logger = logger;
            _templateService = templateService;
            _emailRepository = emailRepository;
        }

        public NotificationChannel ChannelType => NotificationChannel.Email;

        public bool CanHandle(NotificationChannel channel)
        {
            return channel == NotificationChannel.Email;
        }

        public async Task<ChannelResult> SendAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        {
            var result = new ChannelResult
            {
                IsSuccess = false,
                SentAt = DateTime.UtcNow
            };

            try
            {
                if (notification == null)
                {
                    result.ErrorMessage = "Notification message is null";
                    _logger.LogError("Notification message is null");
                    return result;
                }

                // Get user email from metadata or use UserId as email
                var userEmail = GetUserEmail(notification);
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    result.ErrorMessage = "User email not found";
                    _logger.LogError("User email not found for notification {NotificationId}", notification.Id);
                    return result;
                }

                // Generate email content using template service
                var emailContent = await GenerateEmailContentAsync(notification);
                
                // Send email with embedded logo
                await SendEmailWithLogoAsync(userEmail, emailContent.Subject, emailContent.Body, cancellationToken);

                result.IsSuccess = true;
                result.ExternalId = notification.Id; // Use notification ID as external reference
                
                _logger.LogInformation("Email sent successfully for notification {NotificationId} to {Email}", 
                    notification.Id, userEmail);

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to send email for notification {NotificationId}", notification.Id);
                return result;
            }
        }

        public async Task<ChannelResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var result = new ChannelResult
            {
                IsSuccess = false,
                SentAt = DateTime.UtcNow
            };

            try
            {
                if (string.IsNullOrWhiteSpace(to))
                {
                    result.ErrorMessage = "Recipient email is required";
                    _logger.LogError("Recipient email is required");
                    return result;
                }

                if (string.IsNullOrWhiteSpace(subject))
                {
                    result.ErrorMessage = "Email subject is required";
                    _logger.LogError("Email subject is required");
                    return result;
                }

                await SendEmailWithLogoAsync(to, subject, body, cancellationToken);

                result.IsSuccess = true;
                result.ExternalId = Guid.NewGuid().ToString(); // Generate unique ID for tracking
                
                _logger.LogInformation("Email sent successfully to {Email} with subject: {Subject}", to, subject);

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return result;
            }
        }

        // ✅ New method to send email with embedded logo
        private async Task SendEmailWithLogoAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            try
            {
                using var mailMessage = new MailMessage();
                mailMessage.To.Add(to);
                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = true;

                // Create HTML view with embedded logo
                var htmlView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                
                // Attach logo as embedded resource
                await AttachLogoToEmail(htmlView);
                
                mailMessage.AlternateViews.Add(htmlView);

                await _emailRepository.SendEmailAsync(mailMessage);
                
                _logger.LogInformation("Email with embedded logo sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email with logo to {Email}", to);
                throw;
            }
        }

        // ✅ Method to attach logo as embedded resource
        private async Task AttachLogoToEmail(AlternateView htmlView)
        {
            try
            {
                // Path to logo file - try multiple locations
                var logoPaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Logos", "Tasco.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Logos", "Tasco.png"),
                    Path.Combine(Environment.CurrentDirectory, "Assets", "Logos", "Tasco.png"),
                    "./Assets/Logos/Tasco.png",
                    "../Assets/Logos/Tasco.png",
                    "/app/Assets/Logos/Tasco.png"  // Docker path
                };

                foreach (var logoPath in logoPaths)
                {
                    _logger.LogInformation("Checking logo path: {LogoPath}", logoPath);
                    
                    if (File.Exists(logoPath))
                    {
                        // Create linked resource for logo
                        var logoResource = new LinkedResource(logoPath, MediaTypeNames.Image.Png)
                        {
                            ContentId = "tasco_logo"  // This matches the 'cid:tasco_logo' in HTML
                        };

                        htmlView.LinkedResources.Add(logoResource);
                        _logger.LogInformation("Logo attached successfully from: {LogoPath}", logoPath);
                        return;
                    }
                }

                _logger.LogWarning("Logo file not found in any location. Email will be sent without logo.");
                LogAvailableFiles();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error attaching logo to email");
            }
        }

        private void LogAvailableFiles()
        {
            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                _logger.LogInformation("Current directory: {CurrentDir}", currentDir);
                
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _logger.LogInformation("Base directory: {BaseDir}", baseDir);

                // Log files in current directory
                if (Directory.Exists(currentDir))
                {
                    var files = Directory.GetFiles(currentDir, "*", SearchOption.AllDirectories)
                        .Where(f => f.Contains("Assets") || f.Contains("Logo") || f.Contains("Tasco") || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        .Take(10);
                    
                    foreach (var file in files)
                    {
                        _logger.LogInformation("Found: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging available files");
            }
        }

        private string GetUserEmail(NotificationMessage notification)
        {
            try
            {
                // Try to get email from metadata first
                if (notification.Metadata != null && notification.Metadata.ContainsKey("email"))
                {
                    return notification.Metadata["email"]?.ToString() ?? string.Empty;
                }

                // Try to get email from metadata with different keys
                if (notification.Metadata != null && notification.Metadata.ContainsKey("userEmail"))
                {
                    return notification.Metadata["userEmail"]?.ToString() ?? string.Empty;
                }

                // If no email in metadata, assume UserId is an email
                if (IsValidEmail(notification.UserId))
                {
                    return notification.UserId;
                }

                // Last resort: append default domain if UserId looks like username
                if (!string.IsNullOrWhiteSpace(notification.UserId) && !notification.UserId.Contains("@"))
                {
                    _logger.LogWarning("UserId {UserId} is not an email, consider adding email to metadata", notification.UserId);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user email from notification {NotificationId}", notification.Id);
                return string.Empty;
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<(string Subject, string Body)> GenerateEmailContentAsync(NotificationMessage notification)
        {
            try
            {
                // Use template service if available
                if (_templateService != null)
                {
                    var templateResult = await _templateService.GenerateContentAsync(
                        NotificationChannel.Email, 
                        notification.Type, 
                        notification);

                    if (templateResult.IsSuccess)
                    {
                        return (templateResult.Subject, templateResult.Body);
                    }
                    
                    _logger.LogWarning("Template service failed, using default content for notification {NotificationId}: {Error}", 
                        notification.Id, templateResult.ErrorMessage);
                }

                // Fallback to default content generation
                return GenerateDefaultEmailContent(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email content for notification {NotificationId}", notification.Id);
                return GenerateDefaultEmailContent(notification);
            }
        }

        private static (string Subject, string Body) GenerateDefaultEmailContent(NotificationMessage notification)
        {
            var subject = $"[{notification.Type}] {notification.Title}";
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px;'>
                            {notification.Title}
                        </h2>
                        <div style='background: #f8f9fa; padding: 15px; border-left: 4px solid #3498db; margin: 20px 0;'>
                            <p style='margin: 0; font-size: 16px;'>{notification.Message}</p>
                        </div>
                        <div style='margin-top: 20px; padding: 15px; background: #ffffff; border: 1px solid #e9ecef;'>
                            <p style='margin: 0; color: #6c757d; font-size: 14px;'>
                                <strong>Type:</strong> {notification.Type}<br>
                                <strong>Priority:</strong> {notification.Priority}<br>
                                <strong>Created:</strong> {notification.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC
                            </p>
                        </div>
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #e9ecef;'>
                        <p style='color: #6c757d; font-size: 12px; text-align: center; margin: 0;'>
                            This is an automated notification from Tasco System.
                        </p>
                    </div>
                </body>
                </html>";

            return (subject, body);
        }
    }
} 