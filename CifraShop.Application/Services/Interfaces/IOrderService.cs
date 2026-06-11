using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderService
    {
        public Task<Order> CreateCardAsync(string customerLogin);
        public Task<Order?> GetOrderById(int id);
        public Task<List<Order>> GetAllOrder();
        public Task<List<Order>> GetOrdersByLogin(string login);
        public Task<List<Order>> GetOrderByStatus(StatusOrder status);
        public Task<List<Order>> GetOrdersBySum(short sum);
        public Task ChangeOrderStatus(Order order, StatusOrder newStatus);
        public Task ChangeCustomerLogin(Order order, string newLogin);
        public Task DeleteOrder(Order order);
        public Task<Order> AddProductToOrder(int orderId, int productId, short quantity);
        public Task<Order> RemoveOrderItem(int orderItemId);

    }
}
