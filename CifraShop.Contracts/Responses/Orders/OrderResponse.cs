using CifraShop.Contracts.Responses.OrderItem;
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Responses.Orders
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public StatusOrder Status { get; set; }
        public short Sum { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public string CustomerLogin { get; set; }
        public int CustomerId {  get; set; }
        public List<OrderItem.OrderResponse> Items { get; set; }
    }
}
