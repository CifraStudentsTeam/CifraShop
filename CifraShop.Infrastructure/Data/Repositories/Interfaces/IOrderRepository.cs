using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAll();
        Task<Order> GetOrderById(int id);
        Task<List<Order>> GetOrdersByLogin(string login);
        Task<List<Order>> GetOrdersBySum(short sum);
        Task<List<Order>> GetOrdersByStatus(StatusOrder order);
        Task AddOrder(Order orderToAdd);
        Task UpdateOrder(Order orderToUpdate);
        Task DeleteOrder(Order orderToDelete);
    }
}
