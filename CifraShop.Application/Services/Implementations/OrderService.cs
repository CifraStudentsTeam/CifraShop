using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;

        //Констуктор
        public OrderService(IOrderRepository repository)
            => _repository = repository;

        //Получение всех заказов
        public async Task<List<Order>> GetAllOrder()
            => await _repository.UploadingOrderData();

        //Получение заказа по id
        public async Task<Order> GetOrderById(int id)
            => await _repository.GetOrderById(id);

        //Получние заказов по логину
        public async Task<List<Order>> GetOrdersByCustomerLogin(string customerLogin)
            => await _repository.GetOrdersByLogin(customerLogin);

        //Получение заказа по статусу
        public async Task<List<Order>> GetOrdersByStatus(StatusOrder status)
            => await _repository.GetOrdersByStatus(status);

        //Получение заказа по сумме        
        public async Task<List<Order>> GetOrdersBySum(short sum)
            => await _repository.GetOrdersBySum(sum);

        //Создание заказа
        public async Task<Order> CreateOrder(short sum, User customer, List<OrderItem> orderItems)
        {
            var order = new Order
            {
                Status = StatusOrder.Pending,
                Sum = sum,
                DateOfPurchase = DateTime.Now,
                CustomerLogin = customer.Email,
                CustomerId = customer.Id,
                Customer = customer,
                OrderItems = orderItems
            };
            
            await _repository.AddOrder(order);
            return order;
        }

        //Обновление заказа
        public Task UpdateOrder(Order orderToUpdate)
            => _repository.UpdateOrder(orderToUpdate);

        //Удаление заказа
        public async Task DeleteOrder(Order ordeToDelete)
            => await _repository.DeleteOrder(ordeToDelete);
    }
}
