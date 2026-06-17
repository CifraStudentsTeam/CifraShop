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
        
        // Добавление заказа
        public async Task AddOrder(Order orderToAdd)
        {
            await _context.Orders.AddAsync(orderToAdd);
            await _context.SaveChangesAsync();
        }
        
        //Обновление заказа
        public async Task UpdateOrder(Order orderToUpdate)
        {
            _context.Orders.Update(orderToUpdate);
            await _context.SaveChangesAsync();
        }


        //Удаление заказа
        public async Task DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}

