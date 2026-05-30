using CifraShop.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto.Mappers
{
    public static class OrderItemMapper
    {
        public static OrderItemResponce ToResponce(OrderItem orderItem)
        {
            return new OrderItemResponce
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                ProductId = orderItem.ProductId,
                Quantity = orderItem.Quantity,
                Price = orderItem.Price
            };
        }
    }
}
