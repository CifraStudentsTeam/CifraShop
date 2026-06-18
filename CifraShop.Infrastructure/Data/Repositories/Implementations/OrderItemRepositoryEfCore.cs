using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class OrderItemRepositoryEfCore : IOrderItemRepository
    {
        private readonly ApplicationContext _context;

        public OrderItemRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId)
            => await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();

        public async Task<OrderItem> GetOrderItemById(int orderItemId)
            => await _context.OrderItems.SingleOrDefaultAsync(oi => oi.Id == orderItemId);

        public async Task AddOrderItem(OrderItem orderItemToAdd)
        {
            await _context.OrderItems.AddAsync(orderItemToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderItem(OrderItem orderItemToUpdate)
        {
            _context.OrderItems.Update(orderItemToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderItem(OrderItem orderItemToRemove)
        {
            _context.OrderItems.Remove(orderItemToRemove);
            await _context.SaveChangesAsync();
        }
    }
}
