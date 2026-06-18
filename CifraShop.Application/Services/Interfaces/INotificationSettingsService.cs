using CifraShop.Domain.Entities;

namespace CifraShop.Application.Services.Interfaces
{
    public interface INotificationSettingsService
    {
        Task<List<NotificationSettings>> GetAll();
        Task<NotificationSettings?> GetById(int id);
        Task<NotificationSettings?> GetByBranch(string branch);
        Task<NotificationSettings> Create(string email, string telegramBotToken, string telegramChatId, string branch, bool notifyOnNewOrder, bool notifyOnStatusChange, bool notifyOnLowStock, int lowStockThreshold);
        Task Update(NotificationSettings settings);
        Task Delete(NotificationSettings settings);
    }
}
