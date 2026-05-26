using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CifraShop.Domain.Models
{
    public class OrderItem
    {
        public uint Id { get; set; }
        public uint OrderId { get; set; }
        public Order Order { get; set; }
        public uint ProductId { get; set; }
        public Product Product { get; set; }
        public uint Quantity { get; set; }
        public uint Price { get; set; }
    }
}
