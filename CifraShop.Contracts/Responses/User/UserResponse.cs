using CifraShop.Domain.Enums;

namespace CifraShop.Contracts.Responses.User
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public short? Balance { get; set; }
        public UserRole Role { get; set; }
    }
}
