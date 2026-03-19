using ConsoleApp3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Servise
{
    public class StudentService : IStudentService
    {
        private readonly AplicationContext _context = new AplicationContext();

        public StudentService(AplicationContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> UpdatingStudentData()
            => await _context.Students.ToListAsync();

        public async Task<List<Student>> GetStudentByDateOfBirth(DateTime dateOfBirth)
            => await _context.Students.Where(x => x.DateOfBirth == dateOfBirth).ToListAsync();

        public async Task<Student> GetStudentById(int id)
            => await _context.Students.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<Student> GetStudentByLoginName(string loginName)
            => await _context.Students.SingleOrDefaultAsync(x => x.LoginName == loginName);
       
        public async Task<Student> StudentRegister(string loginName, string password, DateTime dateOfBirth)
        {
            var student = new Student
            {
                LoginName = loginName,
                Password = password,
                Balance = 0,
                DateOfBirth = dateOfBirth
            };
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task<Student> StudentAuthentication(string loginName, string password)
        {
            var user = await _context.Students.SingleOrDefaultAsync(x => x.LoginName == loginName && x.Password == password);
            return user;
        }
        public async Task DeleteStudent(Student student)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }
    }
}
