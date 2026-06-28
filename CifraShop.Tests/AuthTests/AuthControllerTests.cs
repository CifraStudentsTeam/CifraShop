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

        // РРЅРёС†РёР°Р»РёР·Р°С†РёСЏ РјРѕРєРѕРІ IUserService Рё IAuthService Рё СЃРѕР·РґР°РЅРёРµ СЌРєР·РµРјРїР»СЏСЂ AuthController РїРµСЂРµРґ РєР°Р¶РґС‹Рј С‚РµСЃС‚РѕРј.
        public AuthControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_userServiceMock.Object, _authServiceMock.Object);
        }

        // РЎРѕР·РґР°РЅРёРµ С‚РµСЃС‚РѕРІРѕРіРѕ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ СЃ СѓРєР°Р·Р°РЅРЅС‹РјРё РїР°СЂР°РјРµС‚СЂР°РјРё РёР»Рё Р·РЅР°С‡РµРЅРёСЏРјРё РїРѕ СѓРјРѕР»С‡Р°РЅРёСЋ.
        private static User CreatingATestUserWithDefaultValues(
            int id = 1,
            string email = "test@email.com",
            string password = "hashed",
            UserRole role = UserRole.Student,
            string? branch = null)
            => new() { Id = id, Email = email, Password = password, Balance = 0, Role = role, Branch = branch };

        // РЈСЃС‚Р°РЅРѕРІРєР° С‚РµСЃС‚РѕРІРѕРіРѕ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ СЃ РїРµСЂРµРґР°РЅРЅС‹РјРё СѓС‚РІРµСЂР¶РґРµРЅРёСЏРјРё (claims)
        private void InstallingAUserWithPassedStatements(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РєРѕСЂСЂРµРєС‚РЅС‹С… СѓС‡РµС‚РЅС‹С… РґР°РЅРЅС‹С… РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 200 OK СЃ С‚РѕРєРµРЅРѕРј Рё РёРЅС„РѕСЂРјР°С†РёРµР№ Рѕ РїРѕР»СЊР·РѕРІР°С‚РµР»Рµ.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РґР»СЏ Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР° РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ РєРѕСЂСЂРµРєС‚РЅР°СЏ СЂРѕР»СЊ Admin РІ РѕС‚РІРµС‚Рµ
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РЅРµСЃСѓС‰РµСЃС‚РІСѓСЋС‰РµРј email РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 401 Unauthorized.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РЅРµРІРµСЂРЅРѕРј РїР°СЂРѕР»Рµ РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 401 Unauthorized.
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
        //РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РЅРµ РІР°Р»РёРґРЅРѕР№ РјРѕРґРµР»Рё Р·Р°РїСЂРѕСЃР° РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 400 Bad Request.
        public async Task CheckingThatANonValidModelReturnsA400Status()
        {
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.Login(new LoginRequest());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РјРµС‚РѕРґ VerifyPassword РІС‹Р·С‹РІР°РµС‚СЃСЏ СЂРѕРІРЅРѕ РѕРґРёРЅ СЂР°Р· СЃ РїРµСЂРµРґР°РЅРЅС‹Рј РїР°СЂРѕР»РµРј Рё С…РµС€РµРј РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїРѕСЃР»Рµ СѓСЃРїРµС€РЅРѕР№ РїСЂРѕРІРµСЂРєРё РїР°СЂРѕР»СЏ РІС‹Р·С‹РІР°РµС‚СЃСЏ РјРµС‚РѕРґ РіРµРЅРµСЂР°С†РёРё С‚РѕРєРµРЅР° СЂРѕРІРЅРѕ РѕРґРёРЅ СЂР°Р·.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РЅРµСѓРґР°С‡РЅРѕРј РІС…РѕРґРµ РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ РѕР±РѕР±С‰С‘РЅРЅРѕРµ СЃРѕРѕР±С‰РµРЅРёРµ Р±РµР· СЂР°СЃРєСЂС‹С‚РёСЏ РґРµС‚Р°Р»РµР№.
        public async Task CheckingThatAGeneralizedMessageIsReturnedWithoutRevalingDetails()
        {
            _userServiceMock.Setup(s => s.GetUserByEmail("x@y.com"))
                .ReturnsAsync((User?)null);

            var result = await _controller.Login(new LoginRequest { Email = "x@y.com", Password = "pass" });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("РќРµРІРµСЂРЅС‹Р№ email РёР»Рё РїР°СЂРѕР»СЊ", unauthorized.Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СѓСЃРїРµС€РЅР°СЏ СЂРµРіРёСЃС‚СЂР°С†РёСЏ РЅРѕРІРѕРіРѕ СЃС‚СѓРґРµРЅС‚Р° РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK СЃ С‚РѕРєРµРЅРѕРј, email Рё СЂРѕР»СЊСЋ Student.
        public async Task CheckingThatASuccessfulRegistrationOfANewStudentReturnsA200OKStatus()
        {
            var user = CreatingATestUserWithDefaultValues(email: "new@email.com");
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com"))
                .ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw", "")).ReturnsAsync(user);
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїР°СЂРѕР»СЊ С…РµС€РёСЂСѓРµС‚СЃСЏ СЂРѕРІРЅРѕ РѕРґРёРЅ СЂР°Р· СЃ РїРµСЂРµРґР°РЅРЅС‹Рј Р·РЅР°С‡РµРЅРёРµРј.
        public async Task CheckingThatThePasswordIsHashedOnce()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw", "")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            _authServiceMock.Verify(s => s.HashPassword("password123"), Times.Once);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЃРѕР·РґР°РЅРёРµ СЃС‚СѓРґРµРЅС‚Р° РІС‹Р·С‹РІР°РµС‚СЃСЏ СЃ РїРµСЂРµРґР°РЅРЅС‹Рј email Рё СѓР¶Рµ Р·Р°С…СЌС€РёСЂРѕРІР°РЅРЅС‹Рј РїР°СЂРѕР»РµРј.
        public async Task CheckingThatTheStudentCreationIsCalledWithAnEmailAndAPassword()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw", "")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            _userServiceMock.Verify(s => s.CreateStudent("new@email.com", "hashed-pw", ""), Times.Once);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїРѕСЃР»Рµ СЃРѕР·РґР°РЅРёСЏ СЃС‚СѓРґРµРЅС‚Р° РІС‹Р·С‹РІР°РµС‚СЃСЏ РјРµС‚РѕРґ UpdateUser СЂРѕРІРЅРѕ РѕРґРёРЅ СЂР°Р·.
        public async Task CheckingThatTheUpdateUserMethodIsCalledOnce()
        {
            var user = CreatingATestUserWithDefaultValues();
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw", "")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            _userServiceMock.Verify(s => s.UpdateUser(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё СЂРµРіРёСЃС‚СЂР°С†РёРё СЃ СѓР¶Рµ Р·Р°РЅСЏС‚С‹Рј email РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 409 Conflict СЃ СѓРєР°Р·Р°РЅРёРµРј СЌС‚РѕРіРѕ email.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РЅРµ РІР°Р»РёРґРЅРѕР№ РјРѕРґРµР»Рё Р·Р°РїСЂРѕСЃР° РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 400 Bad Request.
        public async Task CheckingThatA400StatusIsReturnedWhenTheRequestModeIisInvalid()
        {
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.RegisterStudent(new RegisterRequest());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РІ РѕС‚РІРµС‚Рµ РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ РєРѕСЂСЂРµРєС‚РЅС‹Р№ РёРґРµРЅС‚РёС„РёРєР°С‚РѕСЂ СЃРѕР·РґР°РЅРЅРѕРіРѕ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ.
        public async Task CheckingThatTheCorrectIDIsBeingReturned()
        {
            var user = CreatingATestUserWithDefaultValues(id: 42);
            _userServiceMock.Setup(s => s.GetUserByEmail("new@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateStudent("new@email.com", "hashed-pw", "")).ReturnsAsync(user);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            var result = await _controller.RegisterStudent(new RegisterRequest { Email = "new@email.com", Password = "password123" });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsAssignableFrom<Contracts.Responses.Auth.AuthResponse>(okResult.Value);
            Assert.Equal(42, response.UserId);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё СЃР»РёС€РєРѕРј РєРѕСЂРѕС‚РєРѕРј РїР°СЂРѕР»Рµ РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 400 Bad Request.
        public async Task CheckingThatAShortPasswordReturnsA400Status()
        {
            _controller.ModelState.AddModelError("Password", "РњРёРЅРёРјР°Р»СЊРЅР°СЏ РґР»РёРЅР° вЂ” 6 СЃРёРјРІРѕР»РѕРІ");

            var result = await _controller.RegisterStudent(new RegisterRequest { Email = "a@b.com", Password = "123" });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СѓСЃРїРµС€РЅР°СЏ СЂРµРіРёСЃС‚СЂР°С†РёСЏ Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР° РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK СЃ С‚РѕРєРµРЅРѕРј Рё СЂРѕР»СЊСЋ Admin.
        public async Task CheckingThatASuccessfulAdministratorRegistrationReturnsA200Status()
        {
            var admin = CreatingATestUserWithDefaultValues(email: "admin@email.com", role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateAdmin("admin@email.com", "hashed-pw", "")).ReturnsAsync(admin);
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РїРѕРїС‹С‚РєРµ РїРѕРІС‚РѕСЂРЅРѕР№ СЂРµРіРёСЃС‚СЂР°С†РёРё Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР° СЃ С‚РµРј Р¶Рµ email РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 409 Conflict.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РЅРµ РІР°Р»РёРґРЅРѕР№ РјРѕРґРµР»Рё Р·Р°РїСЂРѕСЃР° РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 400 Bad Request.
        public async Task CheckingThatABadRequestStatusIsReturnedWhenTheRequestModelIsInvalid()
        {
            _controller.ModelState.AddModelError("Email", "Required");

            var result = await _controller.RegisterAdmin(new RegisterRequest());

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё СЂРµРіРёСЃС‚СЂР°С†РёРё Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР° РїР°СЂРѕР»СЊ С…РµС€РёСЂСѓРµС‚СЃСЏ СЂРѕРІРЅРѕ РѕРґРёРЅ СЂР°Р·.
        public async Task CheckingThatTheAdministratorsPasswordIsHashedExactlyOnceDuringRegistration()
        {
            var admin = CreatingATestUserWithDefaultValues(role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateAdmin("admin@email.com", "hashed-pw", "")).ReturnsAsync(admin);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterAdmin(new RegisterRequest { Email = "admin@email.com", Password = "password123" });

            _authServiceMock.Verify(s => s.HashPassword("password123"), Times.Once);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ СЃРѕР·РґР°РЅРёРµ Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР° РІС‹Р·С‹РІР°РµС‚СЃСЏ СЃ РїРµСЂРµРґР°РЅРЅС‹Рј email Рё Р·Р°С…СЌС€РёСЂРѕРІР°РЅРЅС‹Рј РїР°СЂРѕР»РµРј.
        public async Task CheckingThatTheAdministratorCreationIsCalledWithThePassedEmailAndHashedPassword()
        {
            var admin = CreatingATestUserWithDefaultValues(role: UserRole.Admin);
            _userServiceMock.Setup(s => s.GetUserByEmail("admin@email.com")).ReturnsAsync((User?)null);
            _authServiceMock.Setup(s => s.HashPassword("password123")).Returns("hashed-pw");
            _userServiceMock.Setup(s => s.CreateAdmin("admin@email.com", "hashed-pw", "")).ReturnsAsync(admin);
            _userServiceMock.Setup(s => s.UpdateUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _authServiceMock.Setup(s => s.GenerateToken(It.IsAny<User>())).Returns("token");

            await _controller.RegisterAdmin(new RegisterRequest { Email = "admin@email.com", Password = "password123" });

            _userServiceMock.Verify(s => s.CreateAdmin("admin@email.com", "hashed-pw", ""), Times.Once);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РјРµС‚РѕРґ GetCurrentUser РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK СЃ РЅРµРїСѓСЃС‚С‹Рј С‚РµР»РѕРј, СЃРѕРґРµСЂР¶Р°С‰РёРј claims С‚РµРєСѓС‰РµРіРѕ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ GetCurrentUser РІРѕР·РІСЂР°С‰Р°РµС‚ РєРѕСЂСЂРµРєС‚РЅС‹Р№ РёРґРµРЅС‚РёС„РёРєР°С‚РѕСЂ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ РёР· claims.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ GetCurrentUser РІРѕР·РІСЂР°С‰Р°РµС‚ РєРѕСЂСЂРµРєС‚РЅС‹Р№ email РёР· claims.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ GetCurrentUser РІРѕР·РІСЂР°С‰Р°РµС‚ РєРѕСЂСЂРµРєС‚РЅСѓСЋ СЂРѕР»СЊ РёР· claims.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё claim Branch РјРµС‚РѕРґ РІРѕР·РІСЂР°С‰Р°РµС‚ СѓСЃРїРµС€РЅС‹Р№ СЂРµР·СѓР»СЊС‚Р°С‚ СЃ РЅРµРїСѓСЃС‚С‹Рј С‚РµР»РѕРј.
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
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ GetCurrentUser РІРѕР·РІСЂР°С‰Р°РµС‚ СЂРѕР»СЊ Student РґР»СЏ РїРѕР»СЊР·РѕРІР°С‚РµР»СЏ СЃ СЃРѕРѕС‚РІРµС‚СЃС‚РІСѓСЋС‰РµР№ claim.
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
