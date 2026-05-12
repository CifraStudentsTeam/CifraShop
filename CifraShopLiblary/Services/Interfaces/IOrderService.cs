using CifraShopLiblary.Data.DataForModels;
using CifraShopLiblary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopLiblary.Services.Interfaces
{
    public interface IOrderService
    {
        public Task<List<Order>> UploadingOrderData();
        public Task<Order> CreateOrder(StatusOrder order, uint sum, string login);
        public Task<Order> GetOrderById(uint id);
        public Task<List<Order>> GetOrdersByLogin(string login);
        public Task<List<Order>> GetOrdersBySum(uint sum);
        public Task<List<Order>> GetOrdersByStatus(StatusOrder order);
        public Task<Order> ChangeOrderStatus(Order order, StatusOrder status);
        public Task DeleteOrder(Order order);


        public Task<List<OrderItem>> GetOrderItemsByOrderId(uint orderId);
        public Task AddOrderItem(OrderItem orderItem);
        public Task RemoveOrderItem(OrderItem orderItem);
        public Task UpdateOrderItem(OrderItem orderItem);
    }
}
