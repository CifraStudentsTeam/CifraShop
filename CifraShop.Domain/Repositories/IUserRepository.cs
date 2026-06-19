using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsers();
        Task<(List<User> Items, int TotalCount)> GetAllUsersPaged(int page, int pageSize);
        Task<List<User>> GetAllAdmins();
        Task<List<User>> GetAllStudents();
        Task<User?> GetUserById(int id);
        Task<User?> GetUserByEmail(string email);
        Task AddUser(User userToAdd);
        Task UpdateUser(User userToUpdate);
        Task DeleteUser(User userToDelete);
    }
}
