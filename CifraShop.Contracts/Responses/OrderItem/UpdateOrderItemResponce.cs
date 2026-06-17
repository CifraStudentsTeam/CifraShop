using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Responses.OrderItem
{
    public class UpdateOrderItemResponce
    {
        public int Id { get; set; }
        public short Quantity { get; set; }
        public short? Price { get; set; }
    }
}
