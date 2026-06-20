using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Requests.Products
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Price { get; set; }
        public int? Quantity { get; set; }
        public StatusProduct? Status { get; set; }
    }
}
