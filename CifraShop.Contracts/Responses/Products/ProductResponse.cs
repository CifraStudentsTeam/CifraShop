using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Responses.Products
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short Price { get; set; }
        public short Quantity { get; set; }
        public StatusProduct Status { get; set; }
    }
}
