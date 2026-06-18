using CifraShop.Domain.Entities;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsers();
        Task<List<User>> GetAllAdmins();
        Task<List<User>> GetAllStudents();
        Task<User> GetUserById(int id);
        Task<User> GetUserByEmail(string email);
        Task<User> CreateAdmin(string email, string password);
        Task<User> CreateStudent(string email, string password);
        Task UpdateUser(User userToUpdate);
        Task DeleteUser(User userToDelete);
    }
}
