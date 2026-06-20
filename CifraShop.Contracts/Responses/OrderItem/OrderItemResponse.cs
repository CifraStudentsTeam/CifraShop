using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Responses.OrderItem
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public short Price { get; set; }
        public short Quantity { get; set; }
        public int Total => Price * Quantity;
    }
}
