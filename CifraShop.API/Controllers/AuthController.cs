using Microsoft.AspNetCore.Mvc;
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
