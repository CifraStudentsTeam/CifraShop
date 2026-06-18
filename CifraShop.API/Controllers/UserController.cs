using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Users;
using CifraShop.Contracts.Responses.Common;
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
            return Ok(users.Select(u => u.ToResponse()).ToList());
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResponse<UserResponse>>> GetPaged([FromQuery] int page = 0, [FromQuery] int pageSize = 8)
        {
            var paged = await _userService.GetUsersPaged(page, pageSize);
            return Ok(new PagedResponse<UserResponse>
            {
                Items = paged.Items.Select(u => u.ToResponse()).ToList(),
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            });
        }

        [HttpGet("all-admins")]
        public async Task<ActionResult<List<UserResponse>>> GetAllAdmins()
        {
            var users = await _userService.GetAllAdmins();
            return Ok(users.Select(u => u.ToResponse()).ToList());
        }

        [HttpGet("all-students")]
        public async Task<ActionResult<List<UserResponse>>> GetAllStudents()
        {
            var users = await _userService.GetAllStudents();
            return Ok(users.Select(u => u.ToResponse()).ToList());
        }

        [HttpGet("by-id")]
        public async Task<ActionResult<UserResponse>> GetUserById([FromQuery] int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) return NotFound($"Пользователь с id {id} не найден");
            return Ok(user.ToResponse());
        }

        [HttpGet("by-email")]
        public async Task<ActionResult<UserResponse>> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null) return NotFound($"Пользователь с почтой {email} не найден");
            return Ok(user.ToResponse());
        }

        [HttpPost("create-user")]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            var user = await _userService.CreateUser(request.Email, request.Password, "Student");
            return Ok(user.ToResponse());
        }

        [HttpPost("create-admin")]
        public async Task<ActionResult<UserResponse>> CreateAdmin([FromBody] CreateUserRequest request)
        {
            var admin = await _userService.CreateAdmin(request.Email, request.Password);
            return Ok(admin.ToResponse());
        }

        [HttpPost("create-student")]
        public async Task<ActionResult<UserResponse>> CreateStudent([FromBody] CreateUserRequest request)
        {
            var student = await _userService.CreateStudent(request.Email, request.Password);
            return Ok(student.ToResponse());
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromQuery] int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) return NotFound($"Пользователь id {id} не найден");

            if (request.Email != null) user.Email = request.Email;
            if (request.Password != null) user.Password = request.Password;
            if (request.Balance.HasValue) user.Balance = request.Balance.Value;

            await _userService.UpdateUser(user);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) return NotFound($"Пользователь с id {id} не найден");
            await _userService.DeleteUser(user);
            return NoContent();
        }
    }
}
