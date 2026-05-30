using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class UpdateOrderItemRequest
    {
        public uint Id { get; set; }
        public uint OrderId { get; set; }
        public uint ProductId { get; set; }
        public uint Quantity { get; set; }
        public uint Price { get; set; }
    }
}
