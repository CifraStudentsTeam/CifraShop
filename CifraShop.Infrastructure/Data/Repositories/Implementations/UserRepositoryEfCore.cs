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

        //Довалнение пользователя
        public async Task AddUser(User userToAdd)
        {
            await _context.Users.AddAsync(userToAdd);
            await _context.SaveChangesAsync();
        }

        //Обновления пользователя
        public async Task UpdateUser(User userToUpdate)
        {
            _context.Users.Update(userToUpdate);
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
