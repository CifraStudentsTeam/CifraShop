using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CifraShop.Contracts.Responses.Products
{
    public class ProductResponce
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short Price { get; set; }
        public short Quantity { get; set; }
        public StatusProduct Status { get; set; }
    }
}
