using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAll();
        Task<(List<Order> Items, int TotalCount)> GetAllPaged(int page, int pageSize, string? search = null, StatusOrder? status = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<Order?> GetOrderById(int id);
        Task<List<Order>> GetOrdersByCustomerEmail(string email);
        Task<List<Order>> GetOrdersBySum(int sum);
        Task<List<Order>> GetOrdersByStatus(StatusOrder order);
        Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to);
        Task AddOrder(Order orderToAdd);
        Task UpdateOrder(Order orderToUpdate);
        Task UpdateStatusRange(List<int> ids, StatusOrder newStatus);
        Task DeleteOrder(Order orderToDelete);
        Task<Order> CreateOrderInTransaction(
            Order order,
            List<OrderItem> items,
            List<(int ProductId, int Quantity)> stockUpdates);
    }
}
