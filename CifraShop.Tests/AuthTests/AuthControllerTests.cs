using CifraShop.API.Controllers;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Auth;
using CifraShop.Domain.Auth;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CifraShop.Tests.AuthTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        // Инициализация моков IUserService и IAuthService и создание экземпляр AuthController перед каждым тестом.
        public AuthControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_userServiceMock.Object, _authServiceMock.Object);
        }

        // Создание тестового пользователя с указанными параметрами или значениями по умолчанию.
        private static User CreatingATestUserWithDefaultValues(
            int id = 1,
            string email = "test@email.com",
            string password = "hashed",
            UserRole role = UserRole.Student,
            string? branch = null)
            => new() { Id = id, Email = email, Password = password, Balance = 0, Role = role, Branch = branch };

        // Установка тестового пользователя с переданными утверждениями (claims)
        private void InstallingAUserWithPassedStatements(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        // Проверка, что при корректных учетных данных возвращается статус 200 OK с токеном и информацией о пользователе.
        public async Task CheckingThatTheStatisIs200WhenTheCredentialsAreCorrect()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.VerifyPassword("password123", "hashed")).Returns(true);
            _authServiceMock.Setup(s => s.GenerateToken(user)).Returns("jwt-token-123");

            var result = await _controller.Login(new LoginRequest
            {
                Email = "test@email.com",
                Password = "password123"
            });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Contracts.Responses.Auth.AuthResponse>(okResult.Value);
            Assert.Equal("jwt-token-123", response.Token);
            Assert.Equal("test@email.com", response.Email);
            Assert.Equal("Student", response.Role);
            Assert.Equal(1, response.UserId);
        }

        [Fact]
        // Проверка, что для администратора возвращается корректная роль Admin в ответе
        public async Task CheckingThatTheCorrectRoleIsReturnedForTheAdministrator()
        {
            var user = CreatingATestUserWithDefaultValues(role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.VerifyPassword("pass", "hashed")).Returns(true);
            _authServiceMock.Setup(s => s.GenerateToken(user)).Returns("admin-token");

            var result = await _controller.Login(new LoginRequest
            {
                Email = "admin@email.com",
                Password = "pass"
            });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Contracts.Responses.Auth.AuthResponse>(okResult.Value);
            Assert.Equal("Admin", response.Role);
        }

        [Fact]
        // Проверка, что при несуществующем email возвращается статус 401 Unauthorized.
        public async Task CheckingThatNonExistentEmailReturnsA401Status()
        {
            _userServiceMock.Setup(s => s.GetUserByEmail("wrong@email.com"))
                .ReturnsAsync((User?)null);

            var result = await _controller.Login(new LoginRequest
            {
                Email = "wrong@email.com",
                Password = "password123"
            });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        // Проверка, что при неверном пароле возвращается статус 401 Unauthorized.
        public async Task CheckingThatA401StatusIsReturnedWhenThePasswordIsIncorrect()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.VerifyPassword("wrong", "hashed")).Returns(false);

            var result = await _controller.Login(new LoginRequest
            {
                Email = "test@email.com",
                Password = "wrong"
            });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        //Проверка, что при не валидной модели запроса возвращается статус 400 Bad Request.
        public async Task CheckingThatANonValidModelReturnsA400Status()
        {
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.Login(new LoginRequest());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Проверка, что метод VerifyPassword вызывается ровно один раз с переданным паролем и хешем пользователя.
        public async Task CheckingThatTheVerifyPasswordMethodIsCalledExactlyOnce()
        {
            var user = CreatingATestUserWithDefaultValues(password: "stored-hash");
            _userServiceMock.Setup(s => s.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.VerifyPassword("password123", "stored-hash")).Returns(true);
            _authServiceMock.Setup(s => s.GenerateToken(user)).Returns("token");

            await _controller.Login(new LoginRequest { Email = "test@email.com", Password = "password123" });

            _authServiceMock.Verify(s => s.VerifyPassword("password123", "stored-hash"), Times.Once);
        }

        [Fact]
        // Проверка, что после успешной проверки пароля вызывается метод генерации токена ровно один раз.
        public async Task CheckingThatTheTokenGenerationMethodIsCalledExactlyOnce()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _authServiceMock.Setup(s => s.VerifyPassword("password123", "hashed")).Returns(true);
            _authServiceMock.Setup(s => s.GenerateToken(user)).Returns("token");

            await _controller.Login(new LoginRequest { Email = "test@email.com", Password = "password123" });

            _authServiceMock.Verify(s => s.GenerateToken(user), Times.Once);
        }

        [Fact]
        // Проверка, что при неудачном входе возвращается обобщённое сообщение без раскрытия деталей.
        public async Task CheckingThatAGeneralizedMessageIsReturnedWithoutRevalingDetails()
        {
            _userServiceMock.Setup(s => s.GetUserByEmail("x@y.com"))
                .ReturnsAsync((User?)null);

            var result = await _controller.Login(new LoginRequest { Email = "x@y.com", Password = "pass" });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("Неверный email или пароль", unauthorized.Value);
        }

        [Fact]
        // Проверка, что успешная регистрация нового студента возвращает статус 200 OK с токеном, email и ролью Student.
        public async Task CheckingThatASuccessfulRegistrationOfANewStudentReturnsA200OKStatus()
        {
            var user = CreatingATestUserWithDefaultValues(email: "new@email.com");
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com"))
                .ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("jwt-token-456");

            var result = await _controller.RegisterStudent(new RegisterRequest
            {
                Email = "new@email.com",
                Password = "password123"
            });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Contracts.Responses.Auth.AuthResponse>(okResult.Value);
            Assert.Equal("jwt-token-456", response.Token);
            Assert.Equal("new@email.com", response.Email);
            Assert.Equal("Student", response.Role);
        }

        [Fact]
        // Проверка, что пароль хешируется ровно один раз с переданным значением.
        public async Task CheckingThatThePasswordIsHashedOnce()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            _authServiceMock.Verify(s => s.HashPassword("password123"), Times.Once);
        }

        [Fact]
        // Проверка, что создание студента вызывается с переданным email и уже захэшированным паролем.
        public async Task CheckingThatTheStudentCreationIsCalledWithAnEmailAndAPassword()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            _userServiceMock.Verify(s => s.CreateStudent("new@email.com", "hashed-pw"), Times.Once);
        }

        [Fact]
        // Проверка, что после создания студента вызывается метод UpdateUser ровно один раз.
        public async Task CheckingThatTheUpdateUserMethodIsCalledOnce()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            _userServiceMock.Verify(s => s.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        // Проверка, что при регистрации с уже занятым email возвращается статус 409 Conflict с указанием этого email.
        public async Task CheckingThatABusyEmailReturnsA409Status()
        {
            var existing = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("existing@email.com")).ReturnsAsync(existing);

            var result = await _controller.RegisterStudent(new RegisterRequest
            {
                Email = "existing@email.com",
                Password = "password123"
            });

            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("existing@email.com", conflict.Value?.ToString());
        }

        [Fact]
        // Проверка, что при не валидной модели запроса возвращается статус 400 Bad Request.
        public async Task CheckingThatA400StatusIsReturnedWhenTheRequestModeIisInvalid()
        {
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.RegisterStudent(new RegisterRequest());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Проверка, что в ответе возвращается корректный идентификатор созданного пользователя.
        public async Task CheckingThatTheCorrectIDIsBeingReturned()
        {
            var user = CreatingATestUserWithDefaultValues(id: 42);
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            var result = await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Contracts.Responses.Auth.AuthResponse>(okResult.Value);
            Assert.Equal(42, response.UserId);
        }

        [Fact]
        // Проверка, что при слишком коротком пароле возвращается статус 400 Bad Request.
        public async Task CheckingThatAShortPasswordReturnsA400Status()
        {
            _controller.ModelState.AddModelError("Password", "Минимальная длина — 6 символов");

            var result = await _controller.RegisterStudent(new RegisterRequest { Email = "a@b.com", Password = "123" });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Проверка, что успешная регистрация администратора возвращает статус 200 OK с токеном и ролью Admin.
        public async Task CheckingThatASuccessfulAdministratorRegistrationReturnsA200Status()
        {
            var admin = CreatingATestUserWithDefaultValues(email: "admin@email.com", role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateAdmin("admin@email.com", "hashed-pw")).ReturnsAsync(admin);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("jwt-admin-token");

            var result = await _controller.RegisterAdmin(new RegisterRequest
            {
                Email = "admin@email.com",
                Password = "password123"
            });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Contracts.Responses.Auth.AuthResponse>(okResult.Value);
            Assert.Equal("jwt-admin-token", response.Token);
            Assert.Equal("Admin", response.Role);
        }

        [Fact]
        // Проверка, что при попытке повторной регистрации администратора с тем же email возвращается статус 409 Conflict.
        public async Task CheckingThatTheStatusReturns409WhenYouTryToRegisteranAdministrator()
        {
            var existing = CreatingATestUserWithDefaultValues(role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync(existing);

            var result = await _controller.RegisterAdmin(new RegisterRequest
            {
                Email = "admin@email.com",
                Password = "password123"
            });

            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("admin@email.com", conflict.Value?.ToString());
        }

        [Fact]
        // Проверка, что при не валидной модели запроса возвращается статус 400 Bad Request.
        public async Task CheckingThatABadRequestStatusIsReturnedWhenTheRequestModelIsInvalid()
        {
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.RegisterAdmin(new RegisterRequest());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // Проверка, что при регистрации администратора пароль хешируется ровно один раз.
        public async Task CheckingThatTheAdministratorsPasswordIsHashedExactlyOnceDuringRegistration()
        {
            var admin = CreatingATestUserWithDefaultValues(role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateAdmin("admin@email.com", "hashed-pw")).ReturnsAsync(admin);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterAdmin(new RegisterRequest { Email = "admin@email.com", Password = "password123" });

            _authServiceMock.Verify(s => s.HashPassword("password123"), Times.Once);
        }

        [Fact]
        // Проверка, что создание администратора вызывается с переданным email и захэшированным паролем.
        public async Task CheckingThatTheAdministratorCreationIsCalledWithThePassedEmailAndHashedPassword()
        {
            var admin = CreatingATestUserWithDefaultValues(role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateAdmin("admin@email.com", "hashed-pw")).ReturnsAsync(admin);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterAdmin(new RegisterRequest { Email = "admin@email.com", Password = "password123" });

            _userServiceMock.Verify(s => s.CreateAdmin("admin@email.com", "hashed-pw"), Times.Once);
        }

        [Fact]
        // Проверка, что метод GetCurrentUser возвращает статус 200 OK с непустым телом, содержащим claims текущего пользователя.
        public void CheckingThatTheGetCurrentUserMethodReturnsAStatusOf200()
        {
            InstallingAUserWithPassedStatements(
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "a@b.com"),
                new Claim(ClaimTypes.Role, "Student")
            );

            var result = _controller.GetCurrentUser();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // Проверка, что GetCurrentUser возвращает корректный идентификатор пользователя из claims.
        public void CheckingThatGetCurrentUserReturnsAValidID()
        {
            InstallingAUserWithPassedStatements(
                new Claim(ClaimTypes.NameIdentifier, "42"),
                new Claim(ClaimTypes.Email, "a@b.com"),
                new Claim(ClaimTypes.Role, "Student")
            );

            var result = _controller.GetCurrentUser();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var prop = okResult.Value!.GetType().GetProperty("Id")!;
            Assert.Equal("42", prop.GetValue(okResult.Value));
        }

        [Fact]
        // Проверка, что GetCurrentUser возвращает корректный email из claims.
        public void CheckingThatGetCurrentUserReturnsAValidEmailAddress()
        {
            InstallingAUserWithPassedStatements(
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "admin@email.com"),
                new Claim(ClaimTypes.Role, "Admin")
            );

            var result = _controller.GetCurrentUser();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var prop = okResult.Value!.GetType().GetProperty("Email")!;
            Assert.Equal("admin@email.com", prop.GetValue(okResult.Value));
        }

        [Fact]
        // Проверка, что GetCurrentUser возвращает корректную роль из claims.
        public void CheckingThatGetCurrentUserReturnsTheCorrectRole()
        {
            InstallingAUserWithPassedStatements(
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "a@b.com"),
                new Claim(ClaimTypes.Role, "Admin")
            );

            var result = _controller.GetCurrentUser();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var prop = okResult.Value!.GetType().GetProperty("Role")!;
            Assert.Equal("Admin", prop.GetValue(okResult.Value));
        }

        [Fact]
        // Проверка, что при отсутствии claim Branch метод возвращает успешный результат с непустым телом.
        public void CheckingThatTheBranchMethodReturnsASuccessfulResultWhenThereIsNoClaim()
        {
            InstallingAUserWithPassedStatements(
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "a@b.com"),
                new Claim(ClaimTypes.Role, "Student")
            );

            var result = _controller.GetCurrentUser();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // Проверка, что GetCurrentUser возвращает роль Student для пользователя с соответствующей claim.
        public void CheckingThatGetCurrentUserReturnsTheStudentRole()
        {
            InstallingAUserWithPassedStatements(
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "student@email.com"),
                new Claim(ClaimTypes.Role, "Student")
            );

            var result = _controller.GetCurrentUser();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var prop = okResult.Value!.GetType().GetProperty("Role")!;
            Assert.Equal("Student", prop.GetValue(okResult.Value));
        }
    }
}
