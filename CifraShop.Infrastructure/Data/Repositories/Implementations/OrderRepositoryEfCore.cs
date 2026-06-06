using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class OrderRepositoryEfCore : IOrderRepository
    {
        private readonly ApplicationContext _context;

        public OrderRepositoryEfCore(ApplicationContext context)
            => _context = context;

        #region Созданние заказа
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
        #endregion

        #region Чтение данных из БД
        public async Task<List<Order>> UploadingOrderData()
            => await _context.Orders.Include(o => o.OrderItems).ToListAsync();

        public async Task<Order> GetOrderById(int id)
            => await _context.Orders.Include(o => o.OrderItems).SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Order>> GetOrdersByLogin(string login)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.CustomerLogin == login).ToListAsync();

        public async Task<List<Order>> GetOrdersByStatus(StatusOrder order)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Status == order).ToListAsync();

        public async Task<List<Order>> GetOrdersBySum(uint sum)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Sum == sum).ToListAsync();
        public async Task<Order> ChangeOrderStatus(Order order, StatusOrder status)
        {
            order.Status = status;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }
        #endregion

        #region Удаление или обновление заказа
        public async Task DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}

