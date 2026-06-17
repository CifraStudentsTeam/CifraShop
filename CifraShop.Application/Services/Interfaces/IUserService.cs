using CifraShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IUserService
    {
        public Task<List<User>> GetAllUsers();
        public Task<List<User>> GetAllAdmins();
        public Task<List<User>> GetAllStudents();
        public Task<User> GetUserById(int id);
        public Task<User> GetUserByEmail(string email);
        public Task<User> CreateAdmin(string email, string password);
        public Task<User> CreateStudent(string email, string password);
        public Task UpdateUser(User userToUpdate);
        public Task DeleteUser(User userToDelete);
    }
}
