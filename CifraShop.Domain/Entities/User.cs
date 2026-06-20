using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? Balance { get; set; }
        public UserRole Role { get; set; }
        public string? Branch { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
