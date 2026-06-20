using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Requests.OrderItem
{
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public short Quantity { get; set; }
    }
}
