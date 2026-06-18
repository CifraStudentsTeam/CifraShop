using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAll();
        Task<(List<Order> Items, int TotalCount)> GetAllPaged(int page, int pageSize);
        Task<Order> GetOrderById(int id);
        Task<List<Order>> GetOrdersByLogin(string login);
        Task<List<Order>> GetOrdersBySum(short sum);
        Task<List<Order>> GetOrdersByStatus(StatusOrder order);
        Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to);
        Task AddOrder(Order orderToAdd);
        Task UpdateOrder(Order orderToUpdate);
        Task UpdateStatusRange(List<int> ids, StatusOrder newStatus);
        Task DeleteOrder(Order orderToDelete);
    }
}
