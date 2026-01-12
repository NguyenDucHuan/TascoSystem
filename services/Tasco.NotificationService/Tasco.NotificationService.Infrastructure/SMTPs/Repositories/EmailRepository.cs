using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Tasco.NotificationService.Infrastructure.SMTPs.Models;

namespace Tasco.NotificationService.Infrastructure.SMTPs.Repositories
{
    public class EmailRepository : IEmailRepository
    {
        private readonly Email _emailSettings;
        private readonly SmtpClient _smtpClient;

        public EmailRepository(IConfiguration configuration)
        {
            _emailSettings = LoadEmailSettings(configuration);
            _smtpClient = InitializeSmtpClient();
        }

        private Email LoadEmailSettings(IConfiguration configuration)
        {
            var host = configuration["Email:Host"]
                ?? throw new InvalidOperationException("Host is not configured.");

            var portString = configuration["Email:Port"];
            if (!int.TryParse(portString, out var port))
            {
                throw new InvalidOperationException("Port is not configured or invalid.");
            }

            var systemName = configuration["Email:SystemName"]
                ?? throw new InvalidOperationException("SystemName is not configured.");

            var sender = configuration["Email:Sender"]
                ?? throw new InvalidOperationException("Sender is not configured.");

            var password = configuration["Email:Password"]
                ?? throw new InvalidOperationException("Password is not configured.");

            return new Email
            {
                Host = host,
                Port = port,
                SystemName = systemName,
                Sender = sender,
                Password = password
            };
        }

        private SmtpClient InitializeSmtpClient()
        {
            ValidateEmailSettings();

            var smtpClient = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Sender, _emailSettings.Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 30000 // 30 seconds timeout
            };

            // Additional settings for Gmail SMTP
            if (_emailSettings.Host.Contains("gmail"))
            {
                smtpClient.TargetName = "STARTTLS/smtp.gmail.com";
            }

            return smtpClient;
        }

        private void ValidateEmailSettings()
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.Host))
                throw new InvalidOperationException("SMTP Host is not configured.");
            if (_emailSettings.Port <= 0)
                throw new InvalidOperationException("Invalid SMTP Port configuration.");
            if (string.IsNullOrWhiteSpace(_emailSettings.Sender))
                throw new InvalidOperationException("Sender email is not configured.");
            if (string.IsNullOrWhiteSpace(_emailSettings.Password))
                throw new InvalidOperationException("Sender password is not configured.");
            if (string.IsNullOrWhiteSpace(_emailSettings.SystemName))
                throw new InvalidOperationException("System name is not configured.");
        }

        public async Task SendEmailAsync(MailMessage mailMessage)
        {
            if (mailMessage == null)
                throw new ArgumentNullException(nameof(mailMessage));

            try
            {
                mailMessage.From = new MailAddress(_emailSettings.Sender, _emailSettings.SystemName);
                await _smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error while sending email: {ex.Message}", ex);
            }
            finally
            {
                mailMessage?.Dispose();
            }
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}
