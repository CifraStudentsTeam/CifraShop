using CifraShop.Contracts.Requests.OrderItem;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Requests.Orders
{
    public class CreateOrderRequest
    {
        public string CustomerLogin { get; set; } = string.Empty;
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
