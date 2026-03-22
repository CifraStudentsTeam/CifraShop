using ConsoleApp3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Servise
{
    public interface IStudentService
    {
        public Task<List<Student>> UpdatingStudentData();
        public Task<Student> StudentRegister(string loginName, string password, DateTime dateOfBirth);
        public Task<Student> StudentAuthentication(string loginName, string password);
        public Task<Student> GetStudentByLoginName(string loginName);
        public Task<Student> GetStudentById(int id);
        public Task<List<Student>> GetStudentByDateOfBirth(DateTime dateOfBirth);
        public Task DeleteStudent(Student student);

        public Task UpdateStudentBalance(Student student, uint newBalance);
    }
}
