using CifraShop.API.Controllers;
using CifraShop.API.Hubs;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Users;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace CifraShop.Tests.ControllerTests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IHubContext<AdminHub>> _hubContextMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _hubContextMock = new Mock<IHubContext<AdminHub>>();

            var clientsMock = new Mock<IHubClients>();
            clientsMock.Setup(c => c.All).Returns(new Mock<IClientProxy>().Object);
            _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

            _controller = new UserController(_userServiceMock.Object, _hubContextMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithUsers()
        {
            var users = new List<User>
            {
                new() { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Student },
                new() { Id = 2, Email = "c@d.com", Password = "123456", Balance = 100, Role = UserRole.Admin }
            };
            _userServiceMock.Setup(s => s.GetAllUsers()).ReturnsAsync(users);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.User.UserResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetAllPaged_ReturnsOkWithPagedResponse()
        {
            var paged = new Contracts.Responses.Common.PagedResponse<User>
            {
                Items = new List<User> { new() { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Student } },
                Page = 0,
                PageSize = 10,
                TotalCount = 1
            };
            _userServiceMock.Setup(s => s.GetUsersPaged(0, 10, null, null)).ReturnsAsync(paged);

            var result = await _controller.GetPaged(0, 10);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pagedResponse = Assert.IsAssignableFrom<Contracts.Responses.Common.PagedResponse<Contracts.Responses.User.UserResponse>>(okResult.Value);
            Assert.Single(pagedResponse.Items);
            Assert.Equal(1, pagedResponse.TotalCount);
        }

        [Fact]
        public async Task GetAllAdmins_ReturnsOk()
        {
            var admins = new List<User> { new() { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Admin } };
            _userServiceMock.Setup(s => s.GetAllAdmins()).ReturnsAsync(admins);

            var result = await _controller.GetAllAdmins();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.User.UserResponse>>(okResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetAllStudents_ReturnsOk()
        {
            var students = new List<User> { new() { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Student } };
            _userServiceMock.Setup(s => s.GetAllStudents()).ReturnsAsync(students);

            var result = await _controller.GetAllStudents();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.User.UserResponse>>(okResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetUserById_Found_ReturnsOk()
        {
            var user = new User { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Student };
            _userServiceMock.Setup(s => s.GetUserById(1)).ReturnsAsync(user);

            var result = await _controller.GetUserById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Contracts.Responses.User.UserResponse>(okResult.Value);
            Assert.Equal(1, response.Id);
        }

        [Fact]
        public async Task GetUserById_NotFound_ReturnsNotFound()
        {
            _userServiceMock.Setup(s => s.GetUserById(99)).ReturnsAsync((User?)null);

            var result = await _controller.GetUserById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserByEmail_Found_ReturnsOk()
        {
            var user = new User { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Student };
            _userServiceMock.Setup(s => s.GetUserByEmail("a@b.com")).ReturnsAsync(user);

            var result = await _controller.GetUserByEmail("a@b.com");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserByEmail_NotFound_ReturnsNotFound()
        {
            _userServiceMock.Setup(s => s.GetUserByEmail("x@y.com")).ReturnsAsync((User?)null);

            var result = await _controller.GetUserByEmail("x@y.com");

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateAdmin_ValidData_ReturnsOk()
        {
            var user = new User { Id = 1, Email = "admin@email.com", Password = "123456", Balance = 0, Role = UserRole.Admin };
            _userServiceMock.Setup(s => s.CreateAdmin("admin@email.com", "password123", "")).ReturnsAsync(user);

            var result = await _controller.CreateAdmin(new CreateUserRequest { Email = "admin@email.com", Password = "password123" });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Contracts.Responses.User.UserResponse>(okResult.Value);
            Assert.Equal("admin@email.com", response.Email);
        }

        [Fact]
        public async Task CreateStudent_ValidData_ReturnsOk()
        {
            var user = new User { Id = 1, Email = "s@b.com", Password = "123456", Balance = 0, Role = UserRole.Student };
            _userServiceMock.Setup(s => s.CreateStudent("s@b.com", "password123", "")).ReturnsAsync(user);

            var result = await _controller.CreateStudent(new CreateUserRequest { Email = "s@b.com", Password = "password123" });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateUser_Found_ReturnsNoContent()
        {
            var user = new User { Id = 1, Email = "old@email.com", Password = "123456", Balance = 0, Role = UserRole.Student };
            _userServiceMock.Setup(s => s.GetUserById(1)).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateUser(1, new UpdateUserRequest { Email = "new@email.com" });

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("new@email.com", user.Email);
        }

        [Fact]
        public async Task UpdateUser_NotFound_ReturnsNotFound()
        {
            _userServiceMock.Setup(s => s.GetUserById(99)).ReturnsAsync((User?)null);

            var result = await _controller.UpdateUser(99, new UpdateUserRequest { Email = "a@b.com" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUser_UpdatesBalance()
        {
            var user = new User { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Student };
            _userServiceMock.Setup(s => s.GetUserById(1)).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);

            await _controller.UpdateUser(1, new UpdateUserRequest { Balance = 500 });

            Assert.Equal(500, user.Balance);
        }

        [Fact]
        public async Task DeleteUser_Found_ReturnsNoContent()
        {
            var user = new User { Id = 1, Email = "a@b.com", Password = "123456", Balance = 0, Role = UserRole.Student };
            _userServiceMock.Setup(s => s.GetUserById(1)).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.DeleteUser(user)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteUser(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_NotFound_ReturnsNotFound()
        {
            _userServiceMock.Setup(s => s.GetUserById(99)).ReturnsAsync((User?)null);

            var result = await _controller.DeleteUser(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
