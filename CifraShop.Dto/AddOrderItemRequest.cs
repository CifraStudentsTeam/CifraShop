using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class AddOrderItemRequest
    {
        public uint OrderId {  get; set; }
        public uint ProdcutId { get; set; }
        public uint Quantity { get; set; }
        public uint Price { get; set; }
    }
}
