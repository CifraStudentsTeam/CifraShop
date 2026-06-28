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
                new Product { Id = 1, Name = "Кружка", Description = "Кружка с логотипом", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Брелок с логотипом", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
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
                await context.Products.AddAsync(new Product { Id = i, Name = $"Товар {i}", Description = "Описание", Price = i * 10, Quantity = i, Status = StatusProduct.InStock });
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
                new Product { Id = 1, Name = "Кружка", Description = "Белая", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Металлический", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllPaged(0, 10, search: "Кружка");
            Assert.Single(items);
            Assert.Equal("Кружка", items[0].Name);
        }

        [Fact]
        public async Task GetAllPaged_SearchByDescription()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "Кружка", Description = "Белая керамическая", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Металлический", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var (items, _) = await repository.GetAllPaged(0, 10, search: "керамическая");
            Assert.Single(items);
        }

        [Fact]
        public async Task GetAllPaged_FilterByStatus()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Описание", Price = 50, Quantity = 0, Status = StatusProduct.OutOfStock }
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
            var product = new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var result = await repository.GetProductById(1);
            Assert.NotNull(result);
            Assert.Equal("Кружка", result.Name);
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
                new Product { Id = 1, Name = "Кружка", Description = "Белая", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Кружка", Description = "Чёрная", Price = 120, Quantity = 3, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Брелок", Description = "Металл", Price = 50, Quantity = 10, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetProductsByName("Кружка");
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("Кружка", p.Name));
        }

        [Fact]
        public async Task GetProductsByPrice_ReturnProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Описание", Price = 100, Quantity = 10, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Футболка", Description = "Описание", Price = 200, Quantity = 3, Status = StatusProduct.InStock }
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
                new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Описание", Price = 50, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Футболка", Description = "Описание", Price = 200, Quantity = 10, Status = StatusProduct.InStock }
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
                new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Описание", Price = 50, Quantity = 0, Status = StatusProduct.OutOfStock }
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
            var product = new Product { Id = 1, Name = "Новый товар", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await repository.AddProduct(product);

            var result = await context.Products.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Новый товар", result.Name);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProduct()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            product.Name = "Обновлённая кружка";
            product.Price = 150;
            await repository.UpdateProduct(product);

            var updated = await context.Products.FindAsync(1);
            Assert.Equal("Обновлённая кружка", updated.Name);
            Assert.Equal(150, updated.Price);
        }

        [Fact]
        public async Task DeleteProduct_DeletesProduct()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            await repository.DeleteProduct(product);
            var deleted = await context.Products.FindAsync(1);
            Assert.Null(deleted);
        }

        [Fact(Skip = "ExecuteDeleteAsync не поддерживается InMemory-провайдером. Тест требует реальную БД (SQL Server).")]
        public async Task DeleteRange_DeletesMultipleProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Описание", Price = 50, Quantity = 10, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Футболка", Description = "Описание", Price = 200, Quantity = 3, Status = StatusProduct.InStock }
            );
            await context.SaveChangesAsync();

            await repository.DeleteRange(new List<int> { 1, 3 });
            var remaining = await context.Products.ToListAsync();
            Assert.Single(remaining);
            Assert.Equal(2, remaining[0].Id);
        }

        [Fact(Skip = "ExecuteUpdateAsync не поддерживается InMemory-провайдером. Тест требует реальную БД (SQL Server).")]
        public async Task UpdateStatusRange_UpdatesStatusForMultipleProducts()
        {
            using var context = CreateContext();
            var repository = new ProductRepositoryEfCore(context);
            await context.Products.AddRangeAsync(
                new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock },
                new Product { Id = 2, Name = "Брелок", Description = "Описание", Price = 50, Quantity = 10, Status = StatusProduct.InStock },
                new Product { Id = 3, Name = "Футболка", Description = "Описание", Price = 200, Quantity = 0, Status = StatusProduct.OutOfStock }
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
