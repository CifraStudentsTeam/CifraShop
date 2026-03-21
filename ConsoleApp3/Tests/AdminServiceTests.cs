using ConsoleApp3.Models;
using ConsoleApp3.Servise;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleApp3.Tests
{
    public class AdminServiceTests
    {
        private async  Task<AplicationContext> GetDataBaseContext()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var dbContext = new AplicationContext(options);
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            return dbContext;
        }

        [Fact]
        public async Task RegisterAdmin_ShouldAffAdmin()
        {
            var context = await GetDataBaseContext();
            var service = new AdminService(context);

            var admin = await service.RegisterAdmin("John", "Doe", "john@example.com", "pass123");

            Assert.NotNull(admin);
            Assert.Equal("john@example.com", admin.EMail);
            Assert.Single(context.Admins);
        }

        [Fact]
        public async Task GetAdminByEmail_ReturnsCorrecct()
        {
            var context = await GetDataBaseContext();
            var service = new AdminService(context);
            var expected = new Admin { Name = "Jone", EMail = "jane@example.com", Password = "pwd" };
            context.Admins.Add(expected);
            await context.SaveChangesAsync();

            var result = await service.GetAdminByEmail("jane@example.com");

            Assert.NotNull(result);
            Assert.Equal("Jane", result.Name);
        }

        [Fact]
        public async Task AuthenticationAdmin_ValidGredential_ReturnsAdmin()
        {
            var context = await GetDataBaseContext();
            var service = new AdminService(context);
            var admin = new Admin { EMail = "auth@example.com", Password = "secret" };
            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            var result = await service.AuthenticationAdmin("auth@example.com", "secret");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteAdmin_RemoveAdmin()
        {
            var context = await GetDataBaseContext();
            var service = new AdminService(context);
            var admin = new Admin { EMail = "delete@example.com", Password = "x" };
            context.Admins.Add(admin);
            await context.SaveChangesAsync(); 

            await service.DeleteAdmin(admin);

            Assert.Empty(context.Admins);
        }
    }
}
