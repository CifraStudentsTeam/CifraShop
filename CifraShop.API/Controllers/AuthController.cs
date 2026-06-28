<<<<<<< HEAD
using System.Security.Claims;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Auth;
using CifraShop.Contracts.Responses.Auth;
using CifraShop.Domain.Auth;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
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

        public AuthController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
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
            var user = await _userService.CreateStudent(request.Email, hashedPassword);

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
            var user = await _userService.CreateAdmin(request.Email, hashedPassword);

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
    }
}
=======
﻿using Microsoft.AspNetCore.Mvc;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Responses.User;

namespace CifraShop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email и пароль обязательны");

        var user = await _userService.GetUserByEmail(request.Email);
        if (user == null)
            return Unauthorized("Неверный email или пароль");

        if (user.Password != request.Password)
            return Unauthorized("Неверный email или пароль");

        return Ok(user.ToResponse());
    }
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
>>>>>>> 335e25be17000b709c97c1610d615b28dfc15e82
