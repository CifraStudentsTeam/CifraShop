using CifraShop.Application.Services.Implementations;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Moq;

namespace CifraShop.Tests.ServiceTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _repositoryMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _service = new UserService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsUsers()
        {
            var users = new List<User> { new() { Id = 1, Email = "a@b.com" } };
            _repositoryMock.Setup(r => r.GetAllUsers()).ReturnsAsync(users);

            var result = await _service.GetAllUsers();

            Assert.Single(result);
            _repositoryMock.Verify(r => r.GetAllUsers(), Times.Once);
        }

        [Fact]
        public async Task GetAllAdmins_ReturnsAdmins()
        {
            var admins = new List<User> { new() { Id = 1, Role = UserRole.Admin } };
            _repositoryMock.Setup(r => r.GetAllAdmins()).ReturnsAsync(admins);

            var result = await _service.GetAllAdmins();

            Assert.Single(result);
            Assert.Equal(UserRole.Admin, result[0].Role);
        }

        [Fact]
        public async Task GetAllStudents_ReturnsStudents()
        {
            var students = new List<User> { new() { Id = 1, Role = UserRole.Student } };
            _repositoryMock.Setup(r => r.GetAllStudents()).ReturnsAsync(students);

            var result = await _service.GetAllStudents();

            Assert.Single(result);
            Assert.Equal(UserRole.Student, result[0].Role);
        }

        [Fact]
        public async Task GetUserById_ReturnsUser()
        {
            var user = new User { Id = 1, Email = "test@email.com" };
            _repositoryMock.Setup(r => r.GetUserById(1)).ReturnsAsync(user);

            var result = await _service.GetUserById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetUserById_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetUserById(99)).ReturnsAsync((User?)null);

            var result = await _service.GetUserById(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmail_ReturnsUser()
        {
            var user = new User { Id = 1, Email = "test@email.com" };
            _repositoryMock.Setup(r => r.GetUserByEmail("test@email.com")).ReturnsAsync(user);

            var result = await _service.GetUserByEmail("test@email.com");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUsersPaged_ReturnsPagedResponse()
        {
            var users = new List<User> { new() { Id = 1 } };
            _repositoryMock.Setup(r => r.GetAllUsersPaged(0, 10, null, null))
                .ReturnsAsync((users, 1));

            var result = await _service.GetUsersPaged(0, 10);

            Assert.IsType<PagedResponse<User>>(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(0, result.Page);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task GetUsersPaged_NegativePage_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetUsersPaged(-1, 10));
        }

        [Fact]
        public async Task GetUsersPaged_ZeroPageSize_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetUsersPaged(0, 0));
        }

        [Fact]
        public async Task CreateAdmin_ValidData_CreatesAdmin()
        {
            _repositoryMock.Setup(r => r.GetUserByEmail("admin@email.com"))
                .ReturnsAsync((User?)null);
            _repositoryMock.Setup(r => r.AddUser(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateAdmin("admin@email.com", "password123");

            Assert.Equal("admin@email.com", result.Email);
            Assert.Equal(UserRole.Admin, result.Role);
            Assert.Equal(0, result.Balance);
            _repositoryMock.Verify(r => r.AddUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateAdmin_DuplicateEmail_Throws()
        {
            var existing = new User { Id = 1, Email = "admin@email.com" };
            _repositoryMock.Setup(r => r.GetUserByEmail("admin@email.com"))
                .ReturnsAsync(existing);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAdmin("admin@email.com", "password123"));
        }

        [Fact]
        public async Task CreateAdmin_EmptyEmail_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAdmin("", "password123"));
        }

        [Fact]
        public async Task CreateAdmin_ShortPassword_Throws()
        {
            _repositoryMock.Setup(r => r.GetUserByEmail("a@b.com"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateAdmin("a@b.com", "123"));
        }

        [Fact]
        public async Task CreateStudent_ValidData_CreatesStudent()
        {
            _repositoryMock.Setup(r => r.GetUserByEmail("student@email.com"))
                .ReturnsAsync((User?)null);
            _repositoryMock.Setup(r => r.AddUser(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateStudent("student@email.com", "password123");

            Assert.Equal(UserRole.Student, result.Role);
        }

        [Fact]
        public async Task CreateUser_ValidRole_CreatesUser()
        {
            _repositoryMock.Setup(r => r.GetUserByEmail("user@email.com"))
                .ReturnsAsync((User?)null);
            _repositoryMock.Setup(r => r.AddUser(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateUser("user@email.com", "password123", "Admin");

            Assert.Equal(UserRole.Admin, result.Role);
        }

        [Fact]
        public async Task CreateUser_InvalidRole_Throws()
        {
            _repositoryMock.Setup(r => r.GetUserByEmail("user@email.com"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateUser("user@email.com", "password123", "SuperAdmin"));
        }

        [Fact]
        public async Task UpdateUser_ValidData_UpdatesUser()
        {
            var existing = new User { Id = 1, Email = "old@email.com" };
            _repositoryMock.Setup(r => r.GetUserById(1)).ReturnsAsync(existing);
            _repositoryMock.Setup(r => r.UpdateUser(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            var user = new User { Id = 1, Email = "new@email.com", Password = "123456" };
            await _service.UpdateUser(user);

            _repositoryMock.Verify(r => r.UpdateUser(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateUser(null!));
        }

        [Fact]
        public async Task UpdateUser_EmptyEmail_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateUser(new User { Id = 1, Email = "" }));
        }

        [Fact]
        public async Task UpdateUser_NotFound_Throws()
        {
            _repositoryMock.Setup(r => r.GetUserById(99))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateUser(new User { Id = 99, Email = "a@b.com" }));
        }

        [Fact]
        public async Task DeleteUser_DelegatesToRepository()
        {
            var user = new User { Id = 1 };
            _repositoryMock.Setup(r => r.DeleteUser(user)).Returns(Task.CompletedTask);

            await _service.DeleteUser(user);

            _repositoryMock.Verify(r => r.DeleteUser(user), Times.Once);
        }
    }
}
