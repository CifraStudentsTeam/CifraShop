using CifraShop.Domain.Entities;

namespace CifraShop.Domain.Repositories
{
    public interface INotificationSettingsRepository
    {
        Task<List<NotificationSettings>> GetAll();
        Task<NotificationSettings?> GetById(int id);
        Task<NotificationSettings?> GetByBranch(string branch);
        Task Add(NotificationSettings settings);
        Task Update(NotificationSettings settings);
        Task Delete(NotificationSettings settings);
    }
}
