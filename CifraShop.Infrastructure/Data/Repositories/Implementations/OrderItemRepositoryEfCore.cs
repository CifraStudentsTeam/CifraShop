using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class OrderItemRepositoryEfCore : IOrderItemRepository
    {
        private readonly ApplicationContext _context;
        
        public OrderItemRepositoryEfCore(ApplicationContext context)
            => _context = context;

        #region Создание составляющей заказа
        public async Task<OrderItem> CreateOrderItem(Order order, Product product, uint quantity, uint price)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                OrderInOrder = order,
                ProductId = product.Id,
                ProductInOrder = product,
                Quantity = quantity,
                Price = price
            };

            await _context.AddAsync(orderItem);
            await _context.SaveChangesAsync();
            return orderItem;
        }
        #endregion

        #region Чтение данных 
        public async Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId) 
            => await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();

        public async Task<OrderItem> GetOrderItemById(int orderItemId)
            => await _context.OrderItems.SingleOrDefaultAsync(oi => oi.Id == orderItemId);
        #endregion

        #region Добавление составляющей заказа
        public async Task AddOrderItem(OrderItem orderItem)
        {
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Удаление или обновление составляющей заказа
        public async Task RemoveOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrderItem(OrderItem orderItem)
        {
            _context.OrderItems.Update(orderItem);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
