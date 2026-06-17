using CifraShop.Contracts.Responses.OrderItem;
using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Mappings
{
    public static class OrderItemMapping
    {
        public static Responses.OrderItem.OrderResponse ToResponse(this OrderItem item)
        {
            return new Responses.OrderItem.OrderResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductInOrder?.Name ?? string.Empty,
                Price = item.Price,
                Quantity = item.Quantity
            };
        }
    }
}
