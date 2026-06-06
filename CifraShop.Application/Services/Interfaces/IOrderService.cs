using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersForStudentAsync(string studentLogin);
        Task<Order> GetOrderByIdAsync(uint id);
        Task<Order> CreateOrderAsync(string studentLogin, List<OrderItem> items);
        Task UpdateOrderStatusAsync(uint orderId, StatusOrder newStatus);
        Task CancelOrderAsync(uint orderId);
    }
}
