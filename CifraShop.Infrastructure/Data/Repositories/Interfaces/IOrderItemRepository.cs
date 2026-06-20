using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IOrderItemRepository
    {
        public Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId);
        public Task<OrderItem> GetOrderItemById(int orderItemId);
        public Task AddOrderItem(OrderItem orderItemToAdd);
        public Task UpdateOrderItem(OrderItem orderItemToUpdate);
        public Task DeleteOrderItem(OrderItem orderItemToDelete);
    }
}
