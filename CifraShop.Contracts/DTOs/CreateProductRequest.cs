using System;
using System.Collections.Generic;
using System.Text;
using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.DTOs
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public uint Price { get; set; }
        public uint Quantity { get; set; }
        public StatusProduct Status { get; set; }
    }
}
