using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class CreateOrderRequest
    {
        public StatusOrder Status { get; set; }
        public uint Sum { get; set; }
        public string CustomerLogin { get; set; }
    }
}
