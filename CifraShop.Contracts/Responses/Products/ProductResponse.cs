using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Responses.Products
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public short Price { get; set; }
        public short Quantity { get; set; }
        public StatusProduct Status { get; set; }
        public string? ImageUrl { get; set; }
    }
}
