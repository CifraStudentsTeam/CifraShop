using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public StatusProduct Status { get; set; }
        public string? ImageUrl { get; set; }
        public string Branch { get; set; } = string.Empty;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
