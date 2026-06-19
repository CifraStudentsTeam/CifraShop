using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class NotificationSettingsRepositoryEfCore : INotificationSettingsRepository
    {
        private readonly ApplicationContext _context;

        public NotificationSettingsRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<NotificationSettings>> GetAll()
            => await _context.NotificationSettings.ToListAsync();

        public async Task<NotificationSettings?> GetById(int id)
            => await _context.NotificationSettings.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<NotificationSettings?> GetByBranch(string branch)
            => await _context.NotificationSettings.SingleOrDefaultAsync(x => x.Branch == branch);

        public async Task Add(NotificationSettings settings)
        {
            await _context.NotificationSettings.AddAsync(settings);
            await _context.SaveChangesAsync();
        }

        public async Task Update(NotificationSettings settings)
        {
            _context.NotificationSettings.Update(settings);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(NotificationSettings settings)
        {
            _context.NotificationSettings.Remove(settings);
            await _context.SaveChangesAsync();
        }
    }
}
