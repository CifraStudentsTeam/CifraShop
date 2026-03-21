using ConsoleApp3.Models;
using ConsoleApp3.Servise;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleApp3.Tests
{
    public class ProductServiceTests
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
        public async Task CreateProduct_AddsProduct()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var service = new ProductService(context);

            // Act
            var product = await service.CreateProduct("Laptop", "Gaming laptop", 1500, 5, StatusProduct.InStock);

            // Assert
            Assert.NotNull(product);
            Assert.Equal("Laptop", product.Name);
            Assert.Equal((uint)1500, product.Price);
            Assert.Equal((uint)5, product.Quantity);
        }

        [Fact]
        public async Task GetProductsByName_ReturnsCorrectProduct()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var service = new ProductService(context);
            var expected = new Product { Name = "Mouse", Price = 20 };
            context.Product.Add(expected);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetProductsByName("Mouse");

            // Assert
            Assert.NotNull(result);
            
        }

        [Fact]
        public async Task ChangeProductPrice_UpdatesPrice()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var service = new ProductService(context);
            var product = new Product { Name = "Keyboard", Price = 50 };
            context.Product.Add(product);
            await context.SaveChangesAsync();

            // Act
            await service.ChangeProductPrice(product, 45);

            // Assert
            Assert.Equal((uint)45, product.Price);
            var dbProduct = await context.Product.FindAsync(product.Id);
            Assert.Equal((uint)45, dbProduct.Price);
        }
    }
}