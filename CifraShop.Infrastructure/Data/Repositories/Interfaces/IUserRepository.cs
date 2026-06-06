using CifraShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<User> CreateStudent( string email, string password);
        public Task<User> CreateAdmin( string email, string password);
        public Task<List<User>> GetAllUsers();
        public Task<List<User>> GetAllAdmins();
        public Task<List<User>> GetAllStudents();
        public Task<User> GetUserById(int id);
        public Task<User> GetUserByEmail(string email);
        public Task<User> GetUserByEmailAndPassword(string email, string password);
        public Task<List<User>> GetUsersByBalance(uint balance);
        public Task UpdateUser(User user);
        public Task DeleteUser(User user);
    }
}
