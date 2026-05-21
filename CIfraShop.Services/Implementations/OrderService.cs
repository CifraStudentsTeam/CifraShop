using CifraShopLiblary.Data.DataForModels;
using CifraShopLiblary.DataBase.Context;
using CifraShopLiblary.Models;
using CifraShopLiblary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopLiblary.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationContext _context;

        public OrderService(ApplicationContext context)
            => _context = context;

        public async Task<List<Order>> UploadingOrderData()
            => await _context.Orders.Include(o => o.OrderItems).ToListAsync();

        public async Task<Order> GetOrderById(uint id)
            => await _context.Orders.Include(o => o.OrderItems).SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Order>> GetOrdersByLogin(string login)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.CustomerLogin == login).ToListAsync();

        public async Task<List<Order>> GetOrdersByStatus(StatusOrder order)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Status == order).ToListAsync();

        public async Task<List<Order>> GetOrdersBySum(uint sum)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Sum == sum).ToListAsync();

        public async Task<Order> CreateOrder(StatusOrder status, uint sum, string login)
        {
            var order = new Order
            {
                Status = status,
                Sum = sum,
                CustomerLogin = login,
                DateOfPurchase = DateTime.Now
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> ChangeOrderStatus(Order order, StatusOrder status)
        {
            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrderItem>> GetOrderItemsByOrderId(uint orderId)
            => await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();

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
