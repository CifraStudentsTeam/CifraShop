using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Tests.RepositoryTests
{
    public class NotificationSettingsRepositoryTests
    {
        private ApplicationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationContext(options);
        }

        [Fact]
        public async Task GetAll_ReturnsAllSettings()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);
            await context.NotificationSettings.AddRangeAsync(
                new NotificationSettings { Id = 1, Branch = "Р¤РёР»РёР°Р» 1", Email = "filial1@email.com", },
                new NotificationSettings { Id = 2, Branch = "Р¤РёР»РёР°Р» 2", Email = "filial2@email.com", }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetAll();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);

            var result = await repository.GetAll();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetById_ReturnSettings()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);
            var settings = new NotificationSettings { Id = 1, Branch = "Р¤РёР»РёР°Р» 1", Email = "filial1@email.com", NotifyOnNewOrder = true };
            await context.NotificationSettings.AddAsync(settings);
            await context.SaveChangesAsync();

            var result = await repository.GetById(1);
            Assert.NotNull(result);
            Assert.Equal("Р¤РёР»РёР°Р» 1", result.Branch);
            Assert.True(result.NotifyOnNewOrder);
        }

        [Fact]
        public async Task GetById_ReturnNull()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);

            var result = await repository.GetById(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByBranch_ReturnSettings()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);
            await context.NotificationSettings.AddRangeAsync(
                new NotificationSettings { Id = 1, Branch = "Р¤РёР»РёР°Р» 1", Email = "filial1@email.com", },
                new NotificationSettings { Id = 2, Branch = "Р¤РёР»РёР°Р» 2", Email = "filial2@email.com", }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetByBranch("Р¤РёР»РёР°Р» 2");
            Assert.NotNull(result);
            Assert.Equal("Р¤РёР»РёР°Р» 2", result.Branch);
            Assert.Equal("filial2@email.com", result.Email);
        }

        [Fact]
        public async Task GetByBranch_ReturnNullForUnknownBranch()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);

            var result = await repository.GetByBranch("РќРµРёР·РІРµСЃС‚РЅС‹Р№ С„РёР»РёР°Р»");
            Assert.Null(result);
        }

        [Fact]
        public async Task Add_AddsSettings()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);
            var settings = new NotificationSettings { Id = 1, Branch = "Р¤РёР»РёР°Р» 1", Email = "filial1@email.com", };
            await repository.Add(settings);

            var result = await context.NotificationSettings.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Р¤РёР»РёР°Р» 1", result.Branch);
        }

        [Fact]
        public async Task Update_UpdatesSettings()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);
            var settings = new NotificationSettings { Id = 1, Branch = "Р¤РёР»РёР°Р» 1", Email = "old@email.com", LowStockThreshold = 5 };
            await context.NotificationSettings.AddAsync(settings);
            await context.SaveChangesAsync();

            settings.Email = "new@email.com";
            settings.LowStockThreshold = 10;
            await repository.Update(settings);

            var updated = await context.NotificationSettings.FindAsync(1);
            Assert.Equal("new@email.com", updated.Email);
            Assert.Equal(10, updated.LowStockThreshold);
        }

        [Fact]
        public async Task Delete_DeletesSettings()
        {
            using var context = CreateContext();
            var repository = new NotificationSettingsRepositoryEfCore(context);
            var settings = new NotificationSettings { Id = 1, Branch = "Р¤РёР»РёР°Р» 1", Email = "filial1@email.com", };
            await context.NotificationSettings.AddAsync(settings);
            await context.SaveChangesAsync();

            await repository.Delete(settings);
            var deleted = await context.NotificationSettings.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}
