using CifraShop.Domain.Entities;

namespace CifraShop.Domain.Repositories
{
    public interface IAdminActionRepository
    {
        Task<List<AdminAction>> GetLastActions(int count, string? branch = null);
        Task AddAction(AdminAction action);
        Task TrimOldActions(int keepCount);
    }
}
