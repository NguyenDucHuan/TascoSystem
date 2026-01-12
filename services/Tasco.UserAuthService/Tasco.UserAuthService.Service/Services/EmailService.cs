using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Tasco.UserAuthService.Repository.SMTPs.Models;
using Tasco.UserAuthService.Repository.SMTPs.Repositories;
using Tasco.UserAuthService.Service.BusinessModels;
using Tasco.UserAuthService.Service.Services.Interface;
using Tasco.UserAuthService.Service.Exceptions;

namespace Tasco.UserAuthService.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository _emailRepository;

        public EmailService(IEmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
        }

        public async Task SendConfirmationEmailAsync(ConfirmEmailBusiness request)
        {
            var templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates/Emails/Confirmed.html");

            if (!File.Exists(templateFilePath))
            {
                throw new EmailTemplateNotFoundException(templateFilePath);
            }

            var body = await File.ReadAllTextAsync(templateFilePath);
            body = body.Replace("@Model.FullName", request.Email ?? "User")
                  .Replace("@Model.ConfirmationLink", request.ConfirmationLink);

            var mailMessage = new MailMessage
            {
                Subject = "Confirm Your Email Address",
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
            };
            mailMessage.To.Add(new MailAddress(request.Email, request.Email ?? "User"));

            // Tạo một đối tượng AlternateView cho HTML content
            var htmlView = AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, "text/html");

            // Thêm hình ảnh (embedded image)
            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Logos/Tasco.png");
            if (!File.Exists(imagePath))
            {
                throw new EmailLogoNotFoundException(imagePath);
            }

            var logo = new LinkedResource(imagePath, "image/png")
            {
                ContentId = "Logo_Tasco",
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64
            };
            htmlView.LinkedResources.Add(logo);

            mailMessage.AlternateViews.Add(htmlView);

            try
            {
                await _emailRepository.SendEmailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new EmailSendFailedException(ex.Message, ex);
            }
        }
    }
}
