using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto
{
    public class ProductResponce
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public uint Price { get; set; }
        public uint Quantity { get; set; }
        public string Status { get; set; }
        public string? ThePathToTheImage { get; set; }
    }
}
