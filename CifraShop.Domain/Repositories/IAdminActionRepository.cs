using CifraShop.Domain.Entities;

namespace CifraShop.Domain.Repositories
{
    public interface IAdminActionRepository
    {
        Task<List<AdminAction>> GetLastActions(int count);
        Task AddAction(AdminAction action);
    }
}
