using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Responses.Products
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Quantity { get; set; }
        public StatusProduct Status { get; set; }
        public string? ImageUrl { get; set; }
        public string Branch { get; set; } = string.Empty;
    }
}
