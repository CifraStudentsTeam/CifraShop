using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Users;
using CifraShop.Contracts.Responses.OrderItem;
using CifraShop.Contracts.Responses.User;
using CifraShop.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Net.WebSockets;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        //Конструктор
        public UserController(IUserService userService)
            => _userService = userService;

        //Получение всех пользователей
        [HttpGet("/all")]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            var users = await _userService.GetAllUsers();
            var result = new List<UserResponse>();

            foreach (var user in users)
                result.Add(user.ToResponse());

            return Ok(result);
        }

        //Получение всех админов
        [HttpGet("/all-admins")]
        public async Task<ActionResult<List<UserResponse>>> GetAllAdmins()
        {
            var users = await _userService.GetAllAdmins();
            var result = new List<UserResponse>();

            foreach (var user in users)
                result.Add(user.ToResponse());

            return Ok(result);
        }

        //Получение всех студентов
        [HttpGet("/all-students")]
        public async Task<ActionResult<List<UserResponse>>> GetAllStudents()
        {
            var users = await _userService.GetAllStudents();
            var result = new List<UserResponse>();

            foreach (var user in users)
                result.Add(user.ToResponse());

            return Ok(result);
        }

        //Получение пользователя по id
        [HttpGet("by-id")]
        public async Task<ActionResult<UserResponse>> GetUserById([FromQuery] int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound($"Пользователь с id {id} не найден");

            return Ok(user.ToResponse());
        }

        //Получение пользователя по почте
        [HttpGet("by-email")]
        public async Task<ActionResult<UserResponse>> GetUserByEmail([FromQuery] string email)
        {
            var user = await _userService.GetUserByEmail(email);

            if (user == null)
                return NotFound($"Пользователь с почтой {email} не нвйден");

            return Ok(user.ToResponse());
        }

        //Создание админа
        [HttpPost("/create-admin")]
        public async Task<ActionResult<UserResponse>> CreateAdmin([FromBody] CreateUserRequest request)
        {
            var admin = await _userService.CreateAdmin(request.Email, request.Password);

            if (admin == null)
                return StatusCode(500, "Админ не был  создан");

            return Ok(admin.ToResponse());
        }

        //Создание студента
        [HttpPost("/create-student")]
        public async Task<ActionResult<UserResponse>> CreateStudent([FromBody] CreateUserRequest request)
        {
            var student = await _userService.CreateStudent(request.Email, request.Password);

            if (student == null)
                return StatusCode(500, "Студент не был создан");

            return Ok(student.ToResponse());
        }

        //Обновление пользователя
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromQuery] int id ,[FromBody] UpdateUserRequest request)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound($"Пользователь id {id} не был найден");

            if (request.Email != null)
                user.Email = request.Email;

            if (request.Password != null)
                user.Password = request.Password;

            if (request.Balance.HasValue)
                user.Balance = request.Balance.Value;

            await _userService.UpdateUser(user);
            return NoContent();
        }

        //Удаление пользователя
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
                return NotFound($"Пользователь с id {id} не был найден");

            await _userService.DeleteUser(user);
            return NoContent();
        }
        //public Task<List<User>> GetAllUsers();
        //public Task<List<User>> GetAllAdmins();
        //public Task<List<User>> GetAllStudents();
        //public Task<User> GetUserById(int id);
        //public Task<User> GetUserByEmail(string email);
        //public Task<User> CreateAdmin(string email, string password);
        //public Task<User> CreateStudent(string email, string password);
        //public Task UpdateUser(User userToUpdate);
        //public Task DeleteUser(User userToDelete);
    }
}
