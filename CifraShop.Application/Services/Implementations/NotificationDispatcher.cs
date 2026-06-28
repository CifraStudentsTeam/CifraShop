using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace CifraShop.Application.Services.Implementations
{
    public class NotificationDispatcher
    {
        private readonly IEmailService _emailService;
        private readonly INotificationSettingsRepository _settingsRepository;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            IEmailService emailService,
            INotificationSettingsRepository settingsRepository,
            ILogger<NotificationDispatcher> logger)
        {
            _emailService = emailService;
            _settingsRepository = settingsRepository;
            _logger = logger;
        }

        public async Task NotifyNewOrder(string branch, int orderId, string customerEmail, int sum)
        {
            try
            {
                var settings = await _settingsRepository.GetByBranch(branch);
                if (settings == null || !settings.NotifyOnNewOrder) return;

                var recipients = GetRecipients(settings);
                if (!recipients.Any()) return;

                var subject = "Новый заказ #" + orderId;
                var body = "<h2>Новый заказ #" + orderId + "</h2><p><strong>Покупатель:</strong> " + customerEmail + "</p><p><strong>Сумма:</strong> " + sum + " ЦФК</p><p><strong>Филиал:</strong> " + branch + "</p>";

                await _emailService.SendBulkAsync(recipients, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка уведомления о новом заказе #{OrderId}", orderId);
            }
        }

        public async Task NotifyStatusChange(string branch, int orderId, string newStatus)
        {
            try
            {
                var settings = await _settingsRepository.GetByBranch(branch);
                if (settings == null || !settings.NotifyOnStatusChange) return;

                var recipients = GetRecipients(settings);
                if (!recipients.Any()) return;

                var subject = "Статус заказа #" + orderId + " изменён";
                var body = "<h2>Изменение статуса заказа #" + orderId + "</h2><p><strong>Новый статус:</strong> " + newStatus + "</p><p><strong>Филиал:</strong> " + branch + "</p>";

                await _emailService.SendBulkAsync(recipients, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка уведомления об изменении статуса заказа #{OrderId}", orderId);
            }
        }

        public async Task NotifyLowStock(string branch, string productName, int quantity, int threshold)
        {
            try
            {
                var settings = await _settingsRepository.GetByBranch(branch);
                if (settings == null || !settings.NotifyOnLowStock) return;

                var recipients = GetRecipients(settings);
                if (!recipients.Any()) return;

                var subject = "Низкий остаток: " + productName;
                var body = "<h2>Низкий остаток на складе</h2><p><strong>Товар:</strong> " + productName + "</p><p><strong>Остаток:</strong> " + quantity + " шт. (порог: " + threshold + ")</p><p><strong>Филиал:</strong> " + branch + "</p>";

                await _emailService.SendBulkAsync(recipients, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка уведомления о низком остатке товара {ProductName}", productName);
            }
        }

        private static List<string> GetRecipients(CifraShop.Domain.Entities.NotificationSettings settings)
        {
            var recipients = new List<string>();
            if (!string.IsNullOrWhiteSpace(settings.AdminEmails))
                recipients.AddRange(settings.AdminEmails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            if (!string.IsNullOrWhiteSpace(settings.Email))
                recipients.Add(settings.Email);
            return recipients.Distinct().ToList();
        }
    }
}