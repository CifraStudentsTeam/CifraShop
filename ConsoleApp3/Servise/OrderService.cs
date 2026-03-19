using ConsoleApp3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Servise
{
    public class OrderService : IOrderService
    {
        private readonly AplicationContext _context = new AplicationContext();

        public OrderService(AplicationContext context) 
            => _context = context;

        public async Task<List<Order>> UploadingOrderData()
            => await _context.Orders.ToListAsync();
        
        public async Task<Order> GetOrderById(uint id)
            => await _context.Orders.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Order>> GetOrdersByLogin(string login)
            => await _context.Orders.Where(x => x.CustomerLogin == login).ToListAsync();

        public async Task<List<Order>> GetOrdersByStatus(StatusOrder order)
            => await  _context.Orders.Where(x => x.Status == order).ToListAsync();

        public async Task<List<Order>> GetOrdersBySum(uint sum) 
            => await _context.Orders.Where(x =>x.Sum == sum).ToListAsync();

        public async Task<Order> CreateOrder(StatusOrder status, uint sum, string login)
        {
            var order = new Order
            {
                Status = status,
                Sum = sum,
                CustomerLogin = login
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
    }
}
