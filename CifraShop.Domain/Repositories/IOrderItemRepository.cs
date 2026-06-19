using CifraShop.Domain.Entities;

namespace CifraShop.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId);
        Task<OrderItem?> GetOrderItemById(int orderItemId);
        Task AddOrderItem(OrderItem orderItemToAdd);
        Task UpdateOrderItem(OrderItem orderItemToUpdate);
        Task DeleteOrderItem(OrderItem orderItemToDelete);
    }
}
