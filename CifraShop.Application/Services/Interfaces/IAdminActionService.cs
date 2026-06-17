using CifraShop.Domain.Entities;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IAdminActionService
    {
        Task<List<AdminAction>> GetLastActions(int count);
        Task AddAction(string type, string details);
    }
}
