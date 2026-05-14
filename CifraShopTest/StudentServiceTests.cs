using CifraShopLiblary.Models;
using CifraShopTest.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShopTest
{
    public class StudentServiceTests : StudentServiceTestBase
    {
        [Fact]
        public async Task StudentRegister_ValidData_ReturnStudent()
        {
            var loginName = "testuser";
            var password = "password123";
            var dateOfBirth = new DateTime(2000, 1, 1);
            var result = await _service.StudentRegister(loginName, password, dateOfBirth);
            Assert.NotNull(result);
            Assert.Equal(loginName, result.LoginName);
            Assert.Equal(password, result.Password);
            Assert.Equal<uint>(0, result.Balance);
            Assert.Equal(dateOfBirth, result.DateOfBirth);

        }

        [Fact]
        public async Task StudentRegister_AddsToDatabase()
        {
            var loginName = "newstudent";
            await _service.StudentRegister(loginName, "pass", DateTime.Now);
            var student = await _context.Students.FirstOrDefaultAsync(x => x.LoginName == loginName);
            Assert.NotNull(student);
        }

        [Fact]
        public async Task GetStudentById_ExisttingId_ReturnStudent()
        {
            var student = new Student
            {
                LoginName = "user1",
                Password = "pass1",
                DateOfBirth = new DateTime(1995, 5, 5)
            };
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            var result = await _service.GetStudentById(student.Id);
            Assert.NotNull(result);
            Assert.Equal("user1", result.LoginName);
        }

        [Fact]
        public async Task GetStudentById_NonExistingId_ReturnsNull()
        {
            var result = await _service.GetStudentById(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStudentByLoginName_Existingogin_ReturnsStudent()
        {
            var loginName = "specificuser";

            await _context.Students.AddAsync(new Student
            {
                LoginName = loginName,
                Password = "pass",
                DateOfBirth = DateTime.Now
            });

            await _context.SaveChangesAsync();
            var result = await _service.GetStudentByLoginName(loginName);
            Assert.NotNull(result);
            Assert.Equal(loginName, result.LoginName);
        }

        [Fact]
        public async Task StudentAuthentication_ValidCredentials_ReturnsStudent()
        {
            var loginName = "authuser";
            var password = "correcrpass";

            await _context.Students.AddAsync(new Student
            {
                LoginName = loginName,
                Password = password,
                DateOfBirth = DateTime.Now
            });

            await _context.SaveChangesAsync();
            var result = await _service.StudentAuthentication(loginName, password);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task StudentAuthentication_InvalidCredentials_ReturnsNull()
        {
            var result = await _service.StudentAuthentication("wrong", "invalid");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStudentByDAteOfBirth_MAtchingDate_ReturnsCollection()
        {
            var dateofBirth = new DateTime(1990, 3, 15);

            await _context.Students.AddRangeAsync(
                new Student { LoginName = "user1", Password = "pass", DateOfBirth = dateofBirth},
                new Student { LoginName = "user2", Password = "pass", DateOfBirth = dateofBirth},
                new Student { LoginName = "user3", Password = "pass", DateOfBirth = DateTime.Now}
            );
            await _context.SaveChangesAsync();
            var result = await _service.GetStudentByDateOfBirth(dateofBirth);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(dateofBirth, x.DateOfBirth));
        }

        [Fact]
        public async Task UpdateStudentBalance_UpdateCorrectly()
        {
            var student = new Student
            {
                LoginName = "balanceuser",
                Password = "pass",
                DateOfBirth = DateTime.Now,
                Balance = 100
            };

            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            uint newBalance = 250;
            await _service.UpdateStudentBalance(student, newBalance);
            var updatedStudent = await _context.Students.FindAsync(student.Id);
            Assert.NotNull(updatedStudent);
            Assert.Equal(newBalance, updatedStudent.Balance);
        }

        [Fact]
        public async Task DeleteStudent_RemovesFromDatabase()
        {
            var student = new Student
            {
                LoginName = "todelete",
                Password = "pass",
                DateOfBirth = DateTime.Now,
            };

            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
            await _service.DeleteStudent(student);
            var deletedStudent = await _context.Students.FindAsync(student.Id);
            Assert.Null(deletedStudent);
        }

        [Fact]
        public async Task UpdatingStudentData_ReturnAllStudents()
        {
            await _context.Students.AddRangeAsync
            (
                new Student { LoginName = "a", Password = "p", DateOfBirth = DateTime.Now },
                new Student { LoginName = "b", Password = "p", DateOfBirth = DateTime.Now },
                new Student { LoginName = "c", Password = "p", DateOfBirth = DateTime.Now }
            );

            await _context.SaveChangesAsync();
            var result = await _service.UpdatingStudentData();
            Assert.Equal(3, result.Count);
        }
    }
}
