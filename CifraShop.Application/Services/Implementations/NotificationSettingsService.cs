using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;

namespace CifraShop.Application.Services.Implementations
{
    public class NotificationSettingsService : INotificationSettingsService
    {
        private readonly INotificationSettingsRepository _repository;

        public NotificationSettingsService(INotificationSettingsRepository repository)
            => _repository = repository;

        public Task<List<NotificationSettings>> GetAll()
            => _repository.GetAll();

        public Task<NotificationSettings?> GetById(int id)
            => _repository.GetById(id);

        public Task<NotificationSettings?> GetByBranch(string branch)
            => _repository.GetByBranch(branch);

        public async Task<NotificationSettings> Create(string email, string telegramBotToken, string telegramChatId, string branch, bool notifyOnNewOrder, bool notifyOnStatusChange, bool notifyOnLowStock, int lowStockThreshold)
        {
            if (string.IsNullOrWhiteSpace(branch))
                throw new ArgumentException("Филиал обязателен");
            if (lowStockThreshold < 0)
                throw new ArgumentException("Порог уведомления о низком запасе не может быть отрицательным");

            var existing = await _repository.GetByBranch(branch);
            if (existing != null)
                throw new ArgumentException($"Настройки для филиала \"{branch}\" уже существуют");

            var settings = new NotificationSettings
            {
                Email = email ?? string.Empty,
                TelegramBotToken = telegramBotToken ?? string.Empty,
                TelegramChatId = telegramChatId ?? string.Empty,
                Branch = branch,
                NotifyOnNewOrder = notifyOnNewOrder,
                NotifyOnStatusChange = notifyOnStatusChange,
                NotifyOnLowStock = notifyOnLowStock,
                LowStockThreshold = lowStockThreshold
            };

            await _repository.Add(settings);
            return settings;
        }

        public Task Update(NotificationSettings settings)
            => _repository.Update(settings);

        public Task Delete(NotificationSettings settings)
            => _repository.Delete(settings);
    }
}
