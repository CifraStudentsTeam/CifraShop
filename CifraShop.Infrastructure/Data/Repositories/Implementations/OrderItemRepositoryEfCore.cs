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

        //Создание составляющей заказа
        public async Task<OrderItem> CreateOrderItem(Order order, Product product, short quantity, short price)
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
        //Изминение количества состовляющей заказа
        public async Task ChangeOrderItemQuantity(OrderItem orderToChange, short quantity)
        {
            orderToChange.Quantity = quantity;
            _context.OrderItems.Update(orderToChange);
            await _context.SaveChangesAsync();
        }

        //Изминение цены товара
        public async Task ChangeOrderItemPrice(OrderItem orderToChange, short price)
        {
            orderToChange.Price = price;
            _context.OrderItems.Update(orderToChange);
            await _context.SaveChangesAsync();
        }

        //удаление состовляющей заказа
        public async Task RemoveOrderItem(OrderItem orderItemToRemove)
        {
            _context.OrderItems.Remove(orderItemToRemove);
            await _context.SaveChangesAsync();
        }

    }
}
