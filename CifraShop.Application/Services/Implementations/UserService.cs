using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;

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

        public async Task<PagedResponse<User>> GetUsersPaged(int page, int pageSize, string? search = null, UserRole? role = null, string? branch = null)
        {
            if (page < 0)
                throw new ArgumentException("Номер страницы не может быть отрицательным");
            if (pageSize <= 0)
                throw new ArgumentException("Размер страницы должен быть больше 0");

            var (items, total) = await _repository.GetAllUsersPaged(page, pageSize, search, role, branch);
            return new PagedResponse<User> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public Task<User?> GetUserById(int id)
            => _repository.GetUserById(id);

        public Task<User?> GetUserByEmail(string email)
            => _repository.GetUserByEmail(email);

        public async Task<User> CreateAdmin(string email, string password, string branch)
        {
            await ValidateUniqueEmail(email);
            ValidateCredentials(email, password);

            var admin = new User
            {
                Email = email,
                Password = password,
                Role = UserRole.Admin,
                Balance = 0,
                Branch = branch
            };

            await _repository.AddUser(admin);
            return admin;
        }

        public async Task<User> CreateStudent(string email, string password, string branch)
        {
            await ValidateUniqueEmail(email);
            ValidateCredentials(email, password);

            var student = new User
            {
                Email = email,
                Password = password,
                Balance = 0,
                Role = UserRole.Student,
                Branch = branch
            };

            await _repository.AddUser(student);
            return student;
        }

        public async Task<User> CreateUser(string email, string password, string role)
        {
            await ValidateUniqueEmail(email);
            ValidateCredentials(email, password);

            if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var userRole))
                throw new ArgumentException($"Недопустимая роль \"{role}\". Допустимые значения: {string.Join(", ", Enum.GetNames<UserRole>())}");

            var user = new User
            {
                Email = email,
                Password = password,
                Role = userRole,
                Balance = 0
            };

            await _repository.AddUser(user);
            return user;
        }

        public async Task UpdateUser(User userToUpdate)
        {
            if (userToUpdate == null)
                throw new ArgumentNullException(nameof(userToUpdate));

            if (string.IsNullOrWhiteSpace(userToUpdate.Email))
                throw new ArgumentException("Email обязателен");

            var existing = await _repository.GetUserById(userToUpdate.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Пользователь с id {userToUpdate.Id} не найден");

            await _repository.UpdateUser(userToUpdate);
        }

        public Task DeleteUser(User userToDelete)
            => _repository.DeleteUser(userToDelete);

        private async Task ValidateUniqueEmail(string email)
        {
            var existing = await _repository.GetUserByEmail(email);
            if (existing != null)
                throw new ArgumentException($"Пользователь с email {email} уже существует");
        }

        private static void ValidateCredentials(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email обязателен");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль обязателен");
            if (password.Length < 6)
                throw new ArgumentException("Пароль должен содержать минимум 6 символов");
        }
    }
}
