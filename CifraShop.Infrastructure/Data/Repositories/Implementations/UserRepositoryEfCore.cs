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

        //Конструктор
        public UserRepositoryEfCore(ApplicationContext context)
            => _context = context;

        //Создание студента
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

        //Создание админов
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

        //Получение всех пользователей
        public async Task<List<User>> GetAllUsers()
            => await _context.Users.ToListAsync();

        //Получение всех админов
        public async Task<List<User>> GetAllAdmins()
            => await _context.Users.Where(x => x.Role == UserRole.Admin).ToListAsync();

        //Получение всех студентов
        public async Task<List<User>> GetAllStudents()
            => await _context.Users.Where(x => x.Role == UserRole.Student).ToListAsync();

        //Получения пользователя по id
        public async Task<User> GetUserById(int id)
            => await _context.Users.SingleOrDefaultAsync(x => x.Id == id);
        
        //Получения пользователя по почте
        public async Task<User> GetUserByEmail(string email)
            => await _context.Users.SingleOrDefaultAsync(x => x.Email == email);

        //Получения пользователя по почте и паролю
        public async Task<User> GetUserByEmailAndPassword(string email, string password)
            => await _context.Users.SingleOrDefaultAsync(x => x.Email == email && x.Password == password);

        //Получения студентов по балансу
        public async Task<List<User>> GetUsersByBalance(short balance)
            => await _context.Users.Where(x => x.Balance == balance).ToListAsync();

        //Изминение почты пользователя
        public async Task ChangeUserEmail(User userToChange, string email)
        {
            userToChange.Email = email;
            _context.Users.Update(userToChange);
            await _context.SaveChangesAsync();
        }

        //Изминения пароля
        public async Task ChangeUserPassword(User userToChange, string password)
        {
            userToChange.Password = password;
            _context.Users.Update(userToChange);
            await _context.SaveChangesAsync();
        }

        //Изминения баланса
        public async Task ChangeUserBalance(User userToChange, short balance)
        {
            userToChange.Balance = balance;
            _context.Users.Update(userToChange);
            await _context.SaveChangesAsync();
        }

        //Удаление пользователя
        public async Task DeleteUser(User userToDelete)
        {
            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
