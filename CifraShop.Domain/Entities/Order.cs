using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public StatusOrder Status { get; set; }
        public int Sum { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public int CustomerId { get; set; }
        public User Customer { get; set; }
        public string Branch { get; set; } = string.Empty;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<OrderImage> Images { get; set; } = new List<OrderImage>();
    }
}
