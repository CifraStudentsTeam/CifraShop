using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Requests.Orders
{
    public class UpdateOrderRequest
    {
        public StatusOrder? Status { get; set; }
        public short? Sum { get; set; }
    }
}
