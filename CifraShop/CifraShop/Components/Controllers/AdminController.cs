
using CifraShop.Dto;
using CifraShop.Dto.Mappers;
using CIfraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace CifraShop.Components.Controllers
{
    //[Route("[controller]")]
    //[ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
            => _adminService = adminService;

        [HttpGet("/all")]
        public async Task<ActionResult<List<AdminResponse>>> GetAll()
        {
            var admins = await _adminService.UploadingAdminData();
            var response = admins.Select(AdminMapper.ToResponse).ToList();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<AdminResponse>> GetById([FromQuery] int id)
        {
            var admin = await _adminService.GetAdminById((uint)id);

            if (admin == null)
                return NotFound($"Админ с id {id} не был найден.");

            return Ok(AdminMapper.ToResponse(admin));
        }

        [HttpGet("by-email")]
        public async Task<ActionResult<AdminResponse>> GetByEmail([FromQuery] string email)
        {
            var admin = await _adminService.GetAdminByEmail(email);

            if (admin == null)
                return NotFound($"Админ с почтой {email} не был найден");

            return Ok(AdminMapper.ToResponse(admin));
        }

        [HttpPost]
        public async Task<ActionResult<AdminResponse>> Authenticate(AutheticationRequestAdmin request)
        {
            var admin = await _adminService.AuthenticationAdmin(request.EMail, request.Password);

            if (admin == null)
                return Unauthorized("Неверная почта или пароль");

            return Ok(AdminMapper.ToResponse(admin));
        }

        [HttpPost("reqister")]
        public async Task<ActionResult<AdminResponse>> Register(RegisterAdminRequest request)
        {
            var existing = await _adminService.GetAdminByEmail(request.EMail);

            if (existing == null)
                return Conflict($"Админ с почтой {request.EMail} уже существует");

            var newAdmin = await _adminService.RegisterAdmin(
                request.Name,
                request.SurName,
                request.EMail,
                request.Password
            );

            return CreatedAtAction(nameof(GetById), new { id = newAdmin.Id }, AdminMapper.ToResponse((newAdmin)));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var admin = await _adminService.GetAdminById((uint)id);

            if (admin == null)
                return NotFound($"Админ с id {id} не найден");

            await _adminService.DeleteAdmin(admin);
            return NoContent();
        }
    }
}
