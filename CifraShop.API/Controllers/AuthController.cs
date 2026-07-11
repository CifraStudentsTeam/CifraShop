using System.Security.Claims;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Auth;
using CifraShop.Contracts.Responses.Auth;
using CifraShop.Domain.Auth;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly INotificationSettingsRepository _settingsRepository;

        public AuthController(IUserService userService, IAuthService authService, INotificationSettingsRepository settingsRepository)
        {
            _userService = userService;
            _authService = authService;
            _settingsRepository = settingsRepository;
        }

        [HttpPost("guest-token")]
        [AllowAnonymous]
        public ActionResult<AuthResponse> GetGuestToken()
        {
            var guest = new User
            {
                Id = 0,
                Email = "guest@local",
                Role = UserRole.Guest
            };

            var token = _authService.GenerateToken(guest);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = guest.Email,
                Role = guest.Role.ToString(),
                UserId = 0
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetUserByEmail(request.Email);
            if (user == null)
                return Unauthorized("Неверный email или пароль");

            if (!_authService.VerifyPassword(request.Password, user.Password))
                return Unauthorized("Неверный email или пароль");

            var token = _authService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                UserId = user.Id
            });
        }

        [HttpPost("register-student")]
        public async Task<ActionResult<AuthResponse>> RegisterStudent([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userService.GetUserByEmail(request.Email);
            if (existingUser != null)
                return Conflict($"Пользователь с email {request.Email} уже существует");

            var hashedPassword = _authService.HashPassword(request.Password);
            var user = await _userService.CreateStudent(request.Email, hashedPassword, request.Branch ?? "");

            user.Password = hashedPassword;
            await _userService.UpdateUser(user);

            var token = _authService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                UserId = user.Id
            });
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AuthResponse>> RegisterAdmin([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userService.GetUserByEmail(request.Email);
            if (existingUser != null)
                return Conflict($"Пользователь с email {request.Email} уже существует");

            var hashedPassword = _authService.HashPassword(request.Password);
            var user = await _userService.CreateAdmin(request.Email, hashedPassword, request.Branch ?? "");

            user.Password = hashedPassword;
            await _userService.UpdateUser(user);

            var token = _authService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                UserId = user.Id
            });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value
            });
        }

        [HttpGet("branches")]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> GetBranches()
        {
            var settings = await _settingsRepository.GetAll();
            var branches = settings.Select(s => s.Branch).Where(b => !string.IsNullOrEmpty(b)).Distinct().OrderBy(b => b).ToList();
            return Ok(branches);
        }
    }
}
