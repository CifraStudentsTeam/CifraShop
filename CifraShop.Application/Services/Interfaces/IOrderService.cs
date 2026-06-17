using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderService
    {
        public Task<List<Order>> GetAllOrder();
        public Task<Order> GetOrderById(int id);
        public Task<List<Order>> GetOrdersByStatus(StatusOrder status);
        public Task<List<Order>> GetOrdersBySum(short sum);
        public Task<List<Order>> GetOrdersByCustomerLogin(string customerLogin);
        public Task<Order> CreateOrder(short sum, User customer, List<OrderItem> orderItems);
        public Task UpdateOrder(Order orderToUpdate);
        public Task DeleteOrder(Order orderToDelete);
    }
}
