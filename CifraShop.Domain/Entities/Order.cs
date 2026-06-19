using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public StatusOrder Status { get; set; }
        public short Sum { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public int CustomerId { get; set; }
        public User Customer { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
