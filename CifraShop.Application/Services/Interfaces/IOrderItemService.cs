using CifraShop.Domain.Entities;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderItemService
    {
        Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId);
        Task<OrderItem?> GetOrderItemById(int orderItemId);
        Task<OrderItem> CreateOrderItem(int orderId, int productId, int quantity);
        Task UpdateOrderItem(OrderItem orderItemToUpdate);
        Task DeleteOrderItem(OrderItem orderItemToDelete);
    }
}
