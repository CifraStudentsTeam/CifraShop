using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        public Task<List<User>> GetAllUsers();
        public Task<List<User>> GetAllAdmins();
        public Task<List<User>> GetAllStudents();
        public Task<User> GetUserById(int id);
        public Task<User> GetUserByEmail(string email);
        public Task<User> GetUserByEmailAndPassword(string email, string password);
        public Task AddUser(User userToAdd);
        public Task UpdateUser(User userToUpdate);
        public Task DeleteUser(User userToDelete);
    }
}
