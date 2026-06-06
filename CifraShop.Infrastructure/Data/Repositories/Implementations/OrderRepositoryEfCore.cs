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

        //Конструктор
        public OrderRepositoryEfCore(ApplicationContext context)
            => _context = context;

        // Созддание заказа
        public async Task<Order> CreateOrder(StatusOrder status, short sum, string login)

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

        // Создание всех заказов
        public async Task<List<Order>> UploadingOrderData()
            => await _context.Orders.Include(o => o.OrderItems).ToListAsync();

        //Получение заказа по id
        public async Task<Order> GetOrderById(int id)
            => await _context.Orders.Include(o => o.OrderItems).SingleOrDefaultAsync(x => x.Id == id);

        //Получение заказазов по логину
        public async Task<List<Order>> GetOrdersByLogin(string login)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.CustomerLogin == login).ToListAsync();

        //Получение заказов по статусу
        public async Task<List<Order>> GetOrdersByStatus(StatusOrder order)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Status == order).ToListAsync();

        //Получение заказов по сумме
        public async Task<List<Order>> GetOrdersBySum(short sum)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Sum == sum).ToListAsync();

        //Изминение статуса заказа
        public async Task ChangeOrderStatus(Order orderToChange, StatusOrder status)
        {
            orderToChange.Status = status;
            _context.Orders.Update(orderToChange);
            await _context.SaveChangesAsync();
            
        }
        
        //Удаление заказа
        public async Task DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        // изминение суммы заказа
        public async Task ChangeOrderSum(Order orderToChange, short sum)
        {
            orderToChange.Sum = sum;
            _context.Orders.Update(orderToChange);
            await _context.SaveChangesAsync();
        }

        //изминение логина заказчика
        public async Task ChangeCustomerLogin(Order orderToChange, string login)
        {
            orderToChange.CustomerLogin = login;
            _context.Orders.Update(orderToChange);
            await _context.SaveChangesAsync();
        }
    }
}

