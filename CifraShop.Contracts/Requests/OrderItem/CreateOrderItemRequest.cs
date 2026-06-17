using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Requests.OrderItem
{
    public class CreateOrderItemRequest
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public short Quantity { get; set; }
    }
}
