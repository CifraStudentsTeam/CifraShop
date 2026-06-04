using CifraShop.Domain.Enums;
using CifraShop.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class OrderResponce
    {
        public uint Id { get; set; }
        public StatusOrder Status { get; set; }
        public uint Sum { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public string CustomerLogin { get; set; }
        public List<OrderItemResponce> OrderItems { get; set; }
    }
}
