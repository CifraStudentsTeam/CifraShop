using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class UserRepositoryEfCore : IUserRepository
    {
        private readonly ApplicationContext _context;

        public UserRepositoryEfCore(ApplicationContext context)
            => _context = context;

        #region Создание пользователя
        public async Task<User> CreateStudent(string email, string password)
        {
            var student = new User
            {
                Email = email,
                Password = password,
                Role = UserRole.Student,
                Balance = 0
            };

            await _context.Users.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
        }


        public async Task<User> CreateAdmin(string email, string password)
        {
            var admin = new User
            {
                Email = email,
                Password = password,
                Role = UserRole.Admin
            };

            await _context.Users.AddAsync(admin);
            await _context.SaveChangesAsync();
            return admin;
        }
        #endregion

        #region Чтенние данных из бд 

        public async Task<List<User>> GetAllUsers()
            => await _context.Users.ToListAsync();

        public async Task<List<User>> GetAllAdmins()
            => await _context.Users.Where(x => x.Role == UserRole.Admin).ToListAsync();

        public async Task<List<User>> GetAllStudents()
            => await _context.Users.Where(x => x.Role == UserRole.Student).ToListAsync();

        public async Task<User> GetUserById(int id)
            => await _context.Users.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<User> GetUserByEmail(string email)
            => await _context.Users.SingleOrDefaultAsync(x => x.Email == email);

        public async Task<User> GetUserByEmailAndPassword(string email, string password)
            => await _context.Users.SingleOrDefaultAsync(x => x.Email == email && x.Password == password);

        public async Task<List<User>> GetUsersByBalance(uint balance)
            => await _context.Users.Where(x => x.Balance == balance).ToListAsync();
        #endregion

        #region Удалиние или обновление пользователя 
        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
