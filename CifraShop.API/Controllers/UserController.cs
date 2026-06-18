using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Users;
using CifraShop.Contracts.Responses.User;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
            => _userService = userService;

        [HttpGet("all")]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            var users = await _userService.GetAllUsers();
            var result = new List<UserResponse>();

            foreach (var user in users)
                result.Add(user.ToResponse());

            return Ok(result);
        }

        [HttpGet("all-admins")]
        public async Task<ActionResult<List<UserResponse>>> GetAllAdmins()
        {
            var users = await _userService.GetAllAdmins();
            var result = new List<UserResponse>();

            foreach (var user in users)
                result.Add(user.ToResponse());

            return Ok(result);
        }

        [HttpGet("all-students")]
        public async Task<ActionResult<List<UserResponse>>> GetAllStudents()
        {
            var users = await _userService.GetAllStudents();
            var result = new List<UserResponse>();

            foreach (var user in users)
                result.Add(user.ToResponse());

            return Ok(result);
        }

        [HttpGet("by-id")]
        public async Task<ActionResult<UserResponse>> GetUserById([FromQuery] int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound($"Пользователь с id {id} не найден");

            return Ok(user.ToResponse());
        }

        [HttpGet("by-email")]
        public async Task<ActionResult<UserResponse>> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userService.GetUserByEmail(email);

            if (user == null)
                return NotFound($"Пользователь с почтой {email} не найден");

            return Ok(user.ToResponse());
        }

        [HttpPost("create-admin")]
        public async Task<ActionResult<UserResponse>> CreateAdmin([FromBody] CreateUserRequest request)
        {
            var admin = await _userService.CreateAdmin(request.Email, request.Password);

            if (admin == null)
                return StatusCode(500, "Админ не был создан");

            return Ok(admin.ToResponse());
        }

        [HttpPost("create-student")]
        public async Task<ActionResult<UserResponse>> CreateStudent([FromBody] CreateUserRequest request)
        {
            var student = await _userService.CreateStudent(request.Email, request.Password);

            if (student == null)
                return StatusCode(500, "Студент не был создан");

            return Ok(student.ToResponse());
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromQuery] int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound($"Пользователь id {id} не найден");

            if (request.Email != null)
                user.Email = request.Email;

            if (request.Password != null)
                user.Password = request.Password;

            if (request.Balance.HasValue)
                user.Balance = request.Balance.Value;

            await _userService.UpdateUser(user);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound($"Пользователь с id {id} не найден");

            await _userService.DeleteUser(user);
            return NoContent();
        }
    }
}
