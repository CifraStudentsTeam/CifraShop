using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class AdminActionRepositoryEfCore : IAdminActionRepository
    {
        private readonly ApplicationContext _context;

        public AdminActionRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<AdminAction>> GetLastActions(int count, string? branch = null)
        {
            var query = _context.AdminActions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(branch))
                query = query.Where(a => a.Branch == branch);
            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task AddAction(AdminAction action)
        {
            await _context.AdminActions.AddAsync(action);
            await _context.SaveChangesAsync();
        }

        public async Task TrimOldActions(int keepCount)
        {
            var total = await _context.AdminActions.CountAsync();
            if (total <= keepCount) return;
            var idsToDelete = await _context.AdminActions
                .OrderBy(a => a.CreatedAt)
                .Take(total - keepCount)
                .Select(a => a.Id)
                .ToListAsync();
            await _context.AdminActions
                .Where(a => idsToDelete.Contains(a.Id))
                .ExecuteDeleteAsync();
        }
    }
}
