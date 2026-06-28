using CifraShop.Application.Models;
using CifraShop.Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CifraShop.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host))
            {
                _logger.LogWarning("SMTP не настроен, письмо не отправлено: {Subject}", subject);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.None);
                if (!string.IsNullOrWhiteSpace(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Письмо отправлено: {To}, тема: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки письма на {To}", to);
            }
        }

        public async Task SendBulkAsync(List<string> recipients, string subject, string htmlBody)
        {
            foreach (var email in recipients.Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                await SendAsync(email, subject, htmlBody);
            }
        }
    }
}