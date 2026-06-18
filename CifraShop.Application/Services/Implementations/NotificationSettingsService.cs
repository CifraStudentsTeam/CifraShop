using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;

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
            var settings = new NotificationSettings
            {
                Email = email,
                TelegramBotToken = telegramBotToken,
                TelegramChatId = telegramChatId,
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
