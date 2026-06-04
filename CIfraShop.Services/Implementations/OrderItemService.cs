using CifraShop.Data.AppDbContext;
using CifraShop.Domain.Models;
using CIfraShop.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CIfraShop.Services.Implementations
{
    public class OrderItemService : IOrderItemService
    {
        private readonly ApplicationContext _context;
        public async Task<List<OrderItem>> GetOrderItemsByOrderId(uint orderId)
            => await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();
        public async Task<OrderItem> GetOrderItemById(uint id)
            => await _context.OrderItems.SingleOrDefaultAsync(oi => oi.Id == id);

        public async Task AddOrderItem(OrderItem orderItem)
        {
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }

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
    }
}
