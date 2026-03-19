using ConsoleApp3.Models;
using ConsoleApp3.Servise;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleApp3.Tests
{
    public class StudentServiceTests
    {
        private async Task<AplicationContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new AplicationContext(options);
            await dbContext.Database.EnsureCreatedAsync();
            return dbContext;
        }

        [Fact]
        public async Task StudentRegister_AddsStudent()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var service = new StudentService(context);
            var dob = new DateTime(2000, 1, 1);

            // Act
            var student = await service.StudentRegister("alice", "pass", dob);

            // Assert
            Assert.NotNull(student);
            Assert.Equal("alice", student.LoginName);
            Assert.Equal(dob, student.DateOfBirth);
            Assert.Equal((uint)0, student.Balance);
        }

        [Fact]
        public async Task GetStudentByLoginName_ReturnsCorrectStudent()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var service = new StudentService(context);
            var student = new Student { LoginName = "bob", Password = "123" };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetStudentByLoginName("bob");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("bob", result.LoginName);
        }

        [Fact]
        public async Task StudentAuthentication_Valid_ReturnsStudent()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var service = new StudentService(context);
            var student = new Student { LoginName = "charlie", Password = "secret" };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            // Act
            var result = await service.StudentAuthentication("charlie", "secret");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteStudent_RemovesStudent()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var service = new StudentService(context);
            var student = new Student { LoginName = "dave" };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            // Act
            await service.DeleteStudent(student);

            // Assert
            Assert.Empty(context.Students);
        }
    }
}
