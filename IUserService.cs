using CifraShop.Components.Models;

namespace CifraShop.Components.Services
{
    public interface IUserService
    {
        public Task<User> CreateStudent(string name, string surName, string email, string password);
        public Task<User> CreateAdmin(string name, string surname, string email, string password);
        public Task<List<User>> GetAllUsers();
        public Task<List<User>> GetAllAdmins();
        public Task<List<User>> GetAllStudents();  
        public Task<User> GetUserById(uint id);
        public Task<User> GetUserByEmail(string email); 
        public Task<User> GetUserByEmailAndPassword(string email, string password);
        public Task<List<User>> GetUsersByBalance(uint balance);
        public Task UpdateUser(User user);
        public Task DeleteUser(User user);
    }
}
