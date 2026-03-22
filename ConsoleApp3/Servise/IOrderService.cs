using ConsoleApp3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Servise
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


        // Новые методы для OrderItem
        Task<List<OrderItem>> GetOrderItemsByOrderId(uint orderId);
        Task AddOrderItem(OrderItem orderItem);
        Task RemoveOrderItem(OrderItem orderItem);
        Task UpdateOrderItem(OrderItem orderItem);
    }
}
