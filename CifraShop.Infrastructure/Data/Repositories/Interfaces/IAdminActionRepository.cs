using CifraShop.Domain.Entities;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IAdminActionRepository
    {
        Task<List<AdminAction>> GetLastActions(int count);
        Task AddAction(AdminAction action);
    }
}
