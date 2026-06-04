using CifraShop.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto.Mappers
{
    public static class OrderMapper
    {
        public static OrderResponce ToResponce(Order order)
        {
            return new OrderResponce
            {
                Id = order.Id,
                Status = order.Status,
                Sum = order.Sum,
                CustomerLogin = order.CustomerLogin,
                DateOfPurchase = order.DateOfPurchase,
                OrderItems = order.OrderItems?.Select(i => OrderItemMapper.ToResponse(i)).ToList() ?? new List<OrderItemResponce>()
            };
        }
    }
}
