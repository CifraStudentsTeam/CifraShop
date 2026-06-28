using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Tests.RepositoryTests
{
    public class AdminActionRepositoryTests
    {
        private ApplicationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationContext(options);
        }

        [Fact]
        public async Task GetLastActions_ReturnsActionsOrderedDesc()
        {
            using var context = CreateContext();
            var repository = new AdminActionRepositoryEfCore(context);
            await context.AdminActions.AddRangeAsync(
                new AdminAction { Id = 1, ActionType = "Create", Details = "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = new DateTime(2025, 1, 1) },
                new AdminAction { Id = 2, ActionType = "Update", Details = "РћР±РЅРѕРІР»С‘РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = new DateTime(2025, 6, 1) },
                new AdminAction { Id = 3, ActionType = "Delete", Details = "РЈРґР°Р»С‘РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 2", CreatedAt = new DateTime(2025, 3, 1) }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetLastActions(2);
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(3, result[1].Id);
        }

        [Fact]
        public async Task GetLastActions_FiltersByBranch()
        {
            using var context = CreateContext();
            var repository = new AdminActionRepositoryEfCore(context);
            await context.AdminActions.AddRangeAsync(
                new AdminAction { Id = 1, ActionType = "Create", Details = "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = new DateTime(2025, 1, 1) },
                new AdminAction { Id = 2, ActionType = "Update", Details = "РћР±РЅРѕРІР»С‘РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = new DateTime(2025, 6, 1) },
                new AdminAction { Id = 3, ActionType = "Delete", Details = "РЈРґР°Р»С‘РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 2", CreatedAt = new DateTime(2025, 3, 1) }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetLastActions(10, "Р¤РёР»РёР°Р» 1");
            Assert.Equal(2, result.Count);
            Assert.All(result, a => Assert.Equal("Р¤РёР»РёР°Р» 1", a.Branch));
        }

        [Fact]
        public async Task GetLastActions_ReturnsEmptyWhenNoActions()
        {
            using var context = CreateContext();
            var repository = new AdminActionRepositoryEfCore(context);

            var result = await repository.GetLastActions(10);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLastActions_LimitsByCount()
        {
            using var context = CreateContext();
            var repository = new AdminActionRepositoryEfCore(context);
            for (int i = 1; i <= 5; i++)
            {
                await context.AdminActions.AddAsync(new AdminAction { Id = i, ActionType = "Action", Details = $"Р”РµР№СЃС‚РІРёРµ {i}", Branch = "Р¤РёР»РёР°Р»", CreatedAt = DateTime.UtcNow.AddDays(-i) });
            }
            await context.SaveChangesAsync();

            var result = await repository.GetLastActions(3);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task AddAction_AddsAction()
        {
            using var context = CreateContext();
            var repository = new AdminActionRepositoryEfCore(context);
            var action = new AdminAction { Id = 1, ActionType = "Create", Details = "РЎРѕР·РґР°РЅ РЅРѕРІС‹Р№ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = DateTime.UtcNow };
            await repository.AddAction(action);

            var result = await context.AdminActions.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Create", result.ActionType);
            Assert.Equal("РЎРѕР·РґР°РЅ РЅРѕРІС‹Р№ С‚РѕРІР°СЂ", result.Details);
        }

        [Fact]
        public async Task AddAction_MultipleActions()
        {
            using var context = CreateContext();
            var repository = new AdminActionRepositoryEfCore(context);
            await repository.AddAction(new AdminAction { Id = 1, ActionType = "Create", Details = "Р”РµР№СЃС‚РІРёРµ 1", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = DateTime.UtcNow });
            await repository.AddAction(new AdminAction { Id = 2, ActionType = "Update", Details = "Р”РµР№СЃС‚РІРёРµ 2", Branch = "Р¤РёР»РёР°Р» 2", CreatedAt = DateTime.UtcNow });

            var all = await context.AdminActions.ToListAsync();
            Assert.Equal(2, all.Count);
        }
    }
}
