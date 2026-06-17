using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class AdminActionRepositoryEfCore : IAdminActionRepository
    {
        private readonly ApplicationContext _context;

        public AdminActionRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<AdminAction>> GetLastActions(int count)
            => await _context.AdminActions
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();

        public async Task AddAction(AdminAction action)
        {
            _context.AdminActions.Add(action);
            await _context.SaveChangesAsync();
        }
    }
}
