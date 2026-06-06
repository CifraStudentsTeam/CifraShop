using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order OrderInOrder { get; set; }
        public int ProductId { get; set; }
        public Product ProductInOrder { get; set; }
        public uint Quantity { get; set; }
        public uint Price { get; set; }
    }
}
