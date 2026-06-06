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
        public Task<OrderItem> CreateOrderItem(Order order, Product product, short quantity, short price);
        public Task ChangeOrderItemQuantity(OrderItem orderItemToChange, short quantity);
        public Task ChangeOrderItemPrice(OrderItem orderItemToChange, short price);
        public Task AddOrderItem(OrderItem orderItemToAdd);
        public Task RemoveOrderItem(OrderItem orderItemToDelete);
    }
}
