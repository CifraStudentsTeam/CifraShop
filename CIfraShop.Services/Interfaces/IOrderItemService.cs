using CifraShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CIfraShop.Services.Interfaces
{
    public interface IOrderItemService
    {
        public  Task<List<OrderItem>> GetOrderItemsByOrderId(uint orderId);
        public Task<OrderItem> GetOrderItemById(uint orderId);
        public Task AddOrderItem(OrderItem orderItem);
        public Task RemoveOrderItem(OrderItem orderItem);
        public  Task UpdateOrderItem(OrderItem orderItem);
    }
}
