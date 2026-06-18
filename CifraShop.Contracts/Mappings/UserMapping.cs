using CifraShop.Contracts.Responses.User;
using CifraShop.Domain.Entities;

namespace CifraShop.Contracts.Mappings
{
    public static class UserMapping
    {
        public static UserResponse ToResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Balance = user.Balance,
                Role = user.Role
            };
        }
    }
}
