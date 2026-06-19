using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public short Price { get; set; }
        public short Quantity { get; set; }
        public StatusProduct Status { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
