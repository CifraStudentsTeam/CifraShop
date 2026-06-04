using CifraShop.Dto;
using CifraShop.Dto.Mappers;
using CIfraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.Components.Controllers
{
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
            => _studentService = studentService;

        [HttpGet]
        public async Task<ActionResult<List<StudentResponse>>> GetAll()
        {
            var students = await _studentService.UpdatingStudentData();
            var responce = students.Select(StudentMapper.ToResponse).ToList();
            return Ok(responce);
        }

        [HttpGet]
        public async Task<ActionResult<StudentResponse>> GetById([FromQuery] int id)
        {
            var student = await _studentService.GetStudentById((uint)id);

            if (student == null)
                return NotFound($"Студент с id {id} не найден");

            return Ok(StudentMapper.ToResponse(student));
        }

        [HttpGet]
        public async Task<ActionResult<StudentResponse>> GetByLoginName([FromQuery] string loginName)
        {
            var student = await _studentService.GetStudentByLoginName(loginName);

            if (student == null)
                return NotFound($"Студент с логином: {loginName} не найден");

            return Ok(StudentMapper.ToResponse(student));
        }

        [HttpGet]
        public async Task<ActionResult<List<StudentResponse>>> GetByDateOfBirth([FromQuery] DateTime date)
        {
            var students =  await _studentService.GetStudentByDateOfBirth(date);
            var responce = students.Select(StudentMapper.ToResponse).ToList();
            return Ok(responce);
        }

        [HttpPost]
        public async Task<ActionResult<StudentResponse>> Authenticate(AuthenticationStudentRequest request)
        {
            var student = await _studentService.StudentAuthentication(request.LoginName, request.Password);
            if (student == null)
                return Unauthorized("Invalid login name or password.");
            return Ok(StudentMapper.ToResponse(student));
        }


        [HttpPut]
        public async Task<ActionResult<StudentResponse>> UpdateBalance([FromQuery]int id, UpdateBalanceRequest request)
        {
            var student = await _studentService.GetStudentById((uint)id);
            if (student == null)
                return NotFound($"Student with id {id} not found.");
            await _studentService.UpdateStudentBalance(student, request.NewBalance);
            return Ok(StudentMapper.ToResponse(student));
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromQuery]int id)
        {
            var student = await _studentService.GetStudentById((uint)id);
            if (student == null)
                return NotFound($"Student with id {id} not found.");
            await _studentService.DeleteStudent(student);
            return NoContent();
        }
    }
}
