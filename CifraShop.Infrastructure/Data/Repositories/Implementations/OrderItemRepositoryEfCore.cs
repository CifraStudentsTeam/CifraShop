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
        
        //Конструктр 
        public OrderItemRepositoryEfCore(ApplicationContext context)
            => _context = context;

        //Получение составляющей заказа по id заказа
        public async Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId) 
            => await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();

        //Получение соcтавляющей заказа
        public async Task<OrderItem> GetOrderItemById(int orderItemId)
            => await _context.OrderItems.SingleOrDefaultAsync(oi => oi.Id == orderItemId);

        //Добавление составляющей заказа
        public async Task AddOrderItem(OrderItem orderItemToAdd)
        {
            await _context.OrderItems.AddAsync(orderItemToAdd);
            await _context.SaveChangesAsync();
        }

        //Обновление составляющей заказа 
        public async Task UpdateOrderItem(OrderItem orderItemToUpdate)
        {
            _context.OrderItems.Update(orderItemToUpdate);
            await _context.SaveChangesAsync();
        }

        //Удаление состовляющей заказа
        public async Task DeleteOrderItem(OrderItem orderItemToRemove)
        {
            _context.OrderItems.Remove(orderItemToRemove);
            await _context.SaveChangesAsync();
        }

    }
}
