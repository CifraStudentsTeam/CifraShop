using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrders();
        Task<Order> GetOrderById(int id);
        Task<List<Order>> GetOrdersByStatus(StatusOrder status);
        Task<List<Order>> GetOrdersBySum(short sum);
        Task<List<Order>> GetOrdersByCustomerLogin(string customerLogin);
        Task<Order> CreateOrder(short sum, int customerId, string customerLogin, List<OrderItem> orderItems);
        Task UpdateOrder(Order orderToUpdate);
        Task DeleteOrder(Order orderToDelete);
    }
}
