using CifraShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderItemService
    {
        public Task<List<OrderItem>> GetOrderItemsByOrderId(int orderId);
        public Task<OrderItem> GetOrderItemById(int orderItemId);
        public Task CreateOrderItem(Order order, Product product, short quantity);
        public Task UpdateOrderItem(OrderItem orderItemToUpdate);
        public Task DeleteOrderItem(OrderItem orderItemToDelete);
    }
}
//public int OrderId { get; set; }
//public Order OrderInOrder { get; set; }
//public int ProductId { get; set; }
//public Product ProductInOrder { get; set; }
//public short Quantity { get; set; }
//public short Price { get; set; }