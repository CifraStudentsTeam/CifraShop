using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrders();
        Task<PagedResponse<Order>> GetOrdersPaged(int page, int pageSize);
        Task<Order> GetOrderById(int id);
        Task<List<Order>> GetOrdersByStatus(StatusOrder status);
        Task<List<Order>> GetOrdersBySum(short sum);
        Task<List<Order>> GetOrdersByCustomerLogin(string customerLogin);
        Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to);
        Task<Order> CreateOrder(short sum, int customerId, string customerLogin, List<OrderItem> orderItems);
        Task UpdateOrder(Order orderToUpdate);
        Task UpdateStatusRange(List<int> ids, StatusOrder newStatus);
        Task DeleteOrder(Order orderToDelete);
    }
}
