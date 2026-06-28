using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Tests.RepositoryTests
{
    public class UserRepositoryTests
    {
        private ApplicationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationContext(options);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsAllUsers()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            var user1 = new User { Id = 1, Email = "user1@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var user2 = new User { Id = 2, Email = "user2@email.com", Password = "12345678", Balance = 0, Role = UserRole.Admin };
            var user3 = new User { Id = 3, Email = "user3@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.AddRangeAsync(user1, user2, user3);
            await context.SaveChangesAsync();

            var result = await repository.GetAllUsers();
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsEmptyListWhenNoUsers()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);

            var result = await repository.GetAllUsers();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllUsersPaged_ReturnsPagedResult()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            for (int i = 1; i <= 5; i++)
            {
                await context.Users.AddAsync(new User { Id = i, Email = $"user{i}@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student });
            }
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllUsersPaged(0, 2);
            Assert.Equal(2, items.Count);
            Assert.Equal(5, totalCount);
        }

        [Fact]
        public async Task GetAllUsersPaged_FiltersBySearch()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            await context.Users.AddRangeAsync(
                new User { Id = 1, Email = "admin@email.com", Password = "12345678", Balance = 0, Role = UserRole.Admin },
                new User { Id = 2, Email = "student@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student }
            );
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllUsersPaged(0, 10, search: "admin");
            Assert.Single(items);
            Assert.Equal("admin@email.com", items[0].Email);
        }

        [Fact]
        public async Task GetAllUsersPaged_FiltersByRole()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            await context.Users.AddRangeAsync(
                new User { Id = 1, Email = "admin@email.com", Password = "12345678", Balance = 0, Role = UserRole.Admin },
                new User { Id = 2, Email = "student@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student },
                new User { Id = 3, Email = "admin2@email.com", Password = "12345678", Balance = 0, Role = UserRole.Admin }
            );
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllUsersPaged(0, 10, role: UserRole.Admin);
            Assert.Equal(2, items.Count);
            Assert.All(items, u => Assert.Equal(UserRole.Admin, u.Role));
        }

        [Fact]
        public async Task GetAllAdmins_ReturnsOnlyAdmins()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            await context.Users.AddRangeAsync(
                new User { Id = 1, Email = "admin@email.com", Password = "12345678", Balance = 0, Role = UserRole.Admin },
                new User { Id = 2, Email = "student@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetAllAdmins();
            Assert.Single(result);
            Assert.Equal(UserRole.Admin, result[0].Role);
        }

        [Fact]
        public async Task GetAllStudents_ReturnsOnlyStudents()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            await context.Users.AddRangeAsync(
                new User { Id = 1, Email = "admin@email.com", Password = "12345678", Balance = 0, Role = UserRole.Admin },
                new User { Id = 2, Email = "student@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student },
                new User { Id = 3, Email = "student2@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetAllStudents();
            Assert.Equal(2, result.Count);
            Assert.All(result, u => Assert.Equal(UserRole.Student, u.Role));
        }

        [Fact]
        public async Task GetUserById_ReturnUser()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            var user = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 100, Role = UserRole.Student };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var result = await repository.GetUserById(1);
            Assert.NotNull(result);
            Assert.Equal("test@email.com", result.Email);
            Assert.Equal(100, result.Balance);
        }

        [Fact]
        public async Task GetUserById_ReturnNull()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);

            var result = await repository.GetUserById(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByEmail_ReturnUser()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            var user = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Admin };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var result = await repository.GetUserByEmail("test@email.com");
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(UserRole.Admin, result.Role);
        }

        [Fact]
        public async Task GetUserByEmail_ReturnNull()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);

            var result = await repository.GetUserByEmail("nonexistent@email.com");
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUser_AddsUser()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            var user = new User { Id = 1, Email = "new@email.com", Password = "12345678", Balance = 50, Role = UserRole.Student };
            await repository.AddUser(user);

            var result = await context.Users.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("new@email.com", result.Email);
            Assert.Equal(50, result.Balance);
        }

        [Fact]
        public async Task UpdateUser_UpdatesUser()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            var user = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            user.Balance = 500;
            user.Email = "updated@email.com";
            await repository.UpdateUser(user);

            var updated = await context.Users.FindAsync(1);
            Assert.Equal(500, updated.Balance);
            Assert.Equal("updated@email.com", updated.Email);
        }

        [Fact]
        public async Task DeleteUser_DeletesUser()
        {
            using var context = CreateContext();
            var repository = new UserRepositoryEfCore(context);
            var user = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            await repository.DeleteUser(user);
            var deleted = await context.Users.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}
