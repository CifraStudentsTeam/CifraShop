using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CifraShop.Contracts.Mappings
{
    public static class OrderMapping
    {
        public static OrderResponse ToResponce(this Order order, List<OrderItem> items)
        {
            return new OrderResponse
            {
                Id = order.Id,
                Status = order.Status,
                Sum = order.Sum,
                DateOfPurchase = order.DateOfPurchase,
                CustomerLogin = order.CustomerLogin,
                CustomerId = order.CustomerId,
                Items = items.Select(i => i.ToResponse()).ToList()
            };
        }
    }
}
