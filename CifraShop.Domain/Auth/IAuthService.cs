using CifraShop.Domain.Entities;

namespace CifraShop.Domain.Auth
{
    public interface IAuthService
    {
        string GenerateToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
