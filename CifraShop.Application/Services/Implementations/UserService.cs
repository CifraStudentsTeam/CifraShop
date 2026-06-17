using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        //Конструктор
        public UserService(IUserRepository repository)
            => _repository = repository;

        //Получение всех админов
        public Task<List<User>> GetAllAdmins()
            => _repository.GetAllAdmins();

        //Получение всех студентов
        public Task<List<User>> GetAllStudents()
            => _repository.GetAllStudents();

        //Получение всех пользователей
        public Task<List<User>> GetAllUsers()
            => _repository.GetAllUsers();

        //Получение пользователя по id
        public Task<User> GetUserById(int id)
            => _repository.GetUserById(id);

        //Получение пользователя по почте
        public Task<User> GetUserByEmail(string email)
            => _repository.GetUserByEmail(email);

        //Создание админа
        public async Task<User> CreateAdmin(string email, string password)
        {
            var admin = new User
            {
                Email = email,
                Password = password,
                Role = Domain.Enums.UserRole.Admin
            };

            await _repository.AddUser(admin);
            return admin;
        }
        

        //Создание студента
        public async Task<User> CreateStudent(string email, string password)
        {
            var student = new User
            {
                Email = email,
                Password = password,
                Balance = 0,
                Role = Domain.Enums.UserRole.Student
            };

            await _repository.AddUser(student);
            return student;
        }

        //Обновление пользователя
        public Task UpdateUser(User userToUpdate)
            => _repository.UpdateUser(userToUpdate);

        //Удаление пользователя
        public Task DeleteUser(User userToDelete)
            => _repository.DeleteUser(userToDelete);
    }
}
