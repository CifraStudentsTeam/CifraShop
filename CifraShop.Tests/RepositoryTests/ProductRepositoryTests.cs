using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CifraShop.Tests.RepositoryTests
{
    public class ProductRepositoryTests
    {
        private ApplicationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ApplicationContext(options);
        }

        [Fact]
        public async Task GetAll_ReturnsAllProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РљСЂСѓР¶РєР° СЃ Р»РѕРіРѕС‚РёРїРѕРј", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "Р‘СЂРµР»РѕРє СЃ Р»РѕРіРѕС‚РёРїРѕРј", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetAll();
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);

            var result = await repository.GetAll();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPaged_ReturnsPagedResult()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            for (int i = 1; i <= 6; i++)
            {
                await context.Products.AddAsync(new Product { Id = i, Name = $"РўРѕРІР°СЂ {i}", Description = "РћРїРёСЃР°РЅРёРµ", Price = i * 10, Quantity = i, Status = StatusProduct.InStock });
            }
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllPaged(0, 3);
            Assert.Equal(3, items.Count);
            Assert.Equal(6, totalCount);
        }

        [Fact]
        public async Task GetAllPaged_SearchByName()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "Р‘РµР»Р°СЏ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РњРµС‚Р°Р»Р»РёС‡РµСЃРєРёР№", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllPaged(0, 10, search: "РљСЂСѓР¶РєР°");
            Assert.Single(items);
            Assert.Equal("РљСЂСѓР¶РєР°", items[0].Name);
        }

        [Fact]
        public async Task GetAllPaged_SearchByDescription()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "Р‘РµР»Р°СЏ РєРµСЂР°РјРёС‡РµСЃРєР°СЏ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РњРµС‚Р°Р»Р»РёС‡РµСЃРєРёР№", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var (items, _) = await repository.GetAllPaged(0, 10, search: "РєРµСЂР°РјРёС‡РµСЃРєР°СЏ");
            Assert.Single(items);
        }

        [Fact]
        public async Task GetAllPaged_FilterByStatus()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РћРїРёСЃР°РЅРёРµ", Price = 50, Quantity = 0, Status = StatusProduct.OutOfStock }
            );
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllPaged(0, 10, status: StatusProduct.OutOfStock);
            Assert.Single(items);
            Assert.Equal(StatusProduct.OutOfStock, items[0].Status);
        }

        [Fact]
        public async Task GetProductById_ReturnProduct()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var result = await repository.GetProductById(1);
            Assert.NotNull(result);
            Assert.Equal("РљСЂСѓР¶РєР°", result.Name);
        }

        [Fact]
        public async Task GetProductById_ReturnNull()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);

            var result = await repository.GetProductById(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductsByName_ReturnProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "Р‘РµР»Р°СЏ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "РљСЂСѓР¶РєР°", Description = "Р§С‘СЂРЅР°СЏ", Price = 120, Quantity = 3, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Р‘СЂРµР»РѕРє", Description = "РњРµС‚Р°Р»Р»", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetProductsByName("РљСЂСѓР¶РєР°");
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("РљСЂСѓР¶РєР°", p.Name));
        }

        [Fact]
        public async Task GetProductsByPrice_ReturnProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 10, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Р¤СѓС‚Р±РѕР»РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 200, Quantity = 3, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetProductsByPrice(100);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetProductsByQuantity_ReturnProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РћРїРёСЃР°РЅРёРµ", Price = 50, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Р¤СѓС‚Р±РѕР»РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 200, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetProductsByQuantity(5);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetProductsByStatus_ReturnProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РћРїРёСЃР°РЅРёРµ", Price = 50, Quantity = 0, Status = StatusProduct.OutOfStock }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetProductsByStatus(StatusProduct.InStock);
            Assert.Single(result);
            Assert.Equal(StatusProduct.InStock, result[0].Status);
        }

        [Fact]
        public async Task AddProduct_AddsProduct()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РќРѕРІС‹Р№ С‚РѕРІР°СЂ", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await repository.AddProduct(product);

            var result = await context.Products.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("РќРѕРІС‹Р№ С‚РѕРІР°СЂ", result.Name);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProduct()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            product.Name = "РћР±РЅРѕРІР»С‘РЅРЅР°СЏ РєСЂСѓР¶РєР°";
            product.Price = 150;
            await repository.UpdateProduct(product);

            var updated = await context.Products.FindAsync(1);
            Assert.Equal("РћР±РЅРѕРІР»С‘РЅРЅР°СЏ РєСЂСѓР¶РєР°", updated.Name);
            Assert.Equal(150, updated.Price);
        }

        [Fact]
        public async Task DeleteProduct_DeletesProduct()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            await repository.DeleteProduct(product);
            var deleted = await context.Products.FindAsync(1);
            Assert.Null(deleted);
        }

        [Fact(Skip = "ExecuteDeleteAsync РЅРµ РїРѕРґРґРµСЂР¶РёРІР°РµС‚СЃСЏ InMemory-РїСЂРѕРІР°Р№РґРµСЂРѕРј. РўРµСЃС‚ С‚СЂРµР±СѓРµС‚ СЂРµР°Р»СЊРЅСѓСЋ Р‘Р” (SQL Server).")]
        public async Task DeleteRange_DeletesMultipleProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РћРїРёСЃР°РЅРёРµ", Price = 50, Quantity = 10, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Р¤СѓС‚Р±РѕР»РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 200, Quantity = 3, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            await repository.DeleteRange(new List<int> { 1, 3 });
            var remaining = await context.Products.ToListAsync();
            Assert.Single(remaining);
            Assert.Equal(2, remaining[0].Id);
        }

        [Fact(Skip = "ExecuteUpdateAsync РЅРµ РїРѕРґРґРµСЂР¶РёРІР°РµС‚СЃСЏ InMemory-РїСЂРѕРІР°Р№РґРµСЂРѕРј. РўРµСЃС‚ С‚СЂРµР±СѓРµС‚ СЂРµР°Р»СЊРЅСѓСЋ Р‘Р” (SQL Server).")]
        public async Task UpdateStatusRange_UpdatesStatusForMultipleProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РћРїРёСЃР°РЅРёРµ", Price = 50, Quantity = 10, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Р¤СѓС‚Р±РѕР»РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 200, Quantity = 0, Status = StatusProduct.OutOfStock }
            );
            await context.SaveChangesAsync();

            await repository.UpdateStatusRange(new List<int> { 1, 2 }, StatusProduct.ComingSoon);

            var p1 = await context.Products.FindAsync(1);
            var p2 = await context.Products.FindAsync(2);
            var p3 = await context.Products.FindAsync(3);
            Assert.Equal(StatusProduct.ComingSoon, p1.Status);
            Assert.Equal(StatusProduct.ComingSoon, p2.Status);
            Assert.Equal(StatusProduct.OutOfStock, p3.Status);
        }
    }
}
