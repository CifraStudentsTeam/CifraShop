using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;

namespace CifraShop.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
            => _repository = repository;

        public Task<List<User>> GetAllAdmins()
            => _repository.GetAllAdmins();

        public Task<List<User>> GetAllStudents()
            => _repository.GetAllStudents();

        public Task<List<User>> GetAllUsers()
            => _repository.GetAllUsers();

        public async Task<PagedResponse<User>> GetUsersPaged(int page, int pageSize)
        {
            var (items, total) = await _repository.GetAllUsersPaged(page, pageSize);
            return new PagedResponse<User> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public Task<User> GetUserById(int id)
            => _repository.GetUserById(id);

        public Task<User> GetUserByEmail(string email)
            => _repository.GetUserByEmail(email);

        public async Task<User> CreateAdmin(string email, string password)
        {
            var admin = new User
            {
                Email = email,
                Password = password,
                Role = UserRole.Admin
            };

            await _repository.AddUser(admin);
            return admin;
        }

        public async Task<User> CreateStudent(string email, string password)
        {
            var student = new User
            {
                Email = email,
                Password = password,
                Balance = 0,
                Role = UserRole.Student
            };

            await _repository.AddUser(student);
            return student;
        }

        public async Task<User> CreateUser(string email, string password, string role)
        {
            var user = new User
            {
                Email = email,
                Password = password,
                Role = role == "Admin" ? UserRole.Admin : UserRole.Student,
                Balance = 0
            };

            await _repository.AddUser(user);
            return user;
        }

        public Task UpdateUser(User userToUpdate)
            => _repository.UpdateUser(userToUpdate);

        public Task DeleteUser(User userToDelete)
            => _repository.DeleteUser(userToDelete);
    }
}
