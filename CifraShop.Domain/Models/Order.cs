
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CifraShop.Domain.Models
{
    public class Order
    {

        public uint Id { get; set; }
        public StatusOrder Status { get; set; }
        public uint Sum { get; set; }
        public DateTime DateOfPurchase { get; set; } = DateTime.Now;
        public string CustomerLogin { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
