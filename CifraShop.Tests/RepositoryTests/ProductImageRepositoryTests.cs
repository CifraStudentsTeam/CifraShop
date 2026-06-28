using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Tests.RepositoryTests
{
    public class ProductImageRepositoryTests
    {
        private ApplicationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationContext(options);
        }

        [Fact]
        public async Task GetByProductId_ReturnsImagesForProduct()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            var product2 = new Product { Id = 2, Name = "Р‘СЂРµР»РѕРє", Description = "РћРїРёСЃР°РЅРёРµ", Price = 50, Quantity = 10, Status = StatusProduct.InStock };
            await context.Products.AddRangeAsync(product, product2);
            await context.ProductImages.AddRangeAsync(
                new ProductImage { Id = 1, ProductId = 1, Product = product, FileName = "img1.png", IsPrimary = true, SortOrder = 0 },
                new ProductImage { Id = 2, ProductId = 1, Product = product, FileName = "img2.png", IsPrimary = false, SortOrder = 1 },
                new ProductImage { Id = 3, ProductId = 2, Product = product2, FileName = "img3.png", IsPrimary = true, SortOrder = 0 }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetByProductId(1);
            Assert.Equal(2, result.Count);
            Assert.All(result, i => Assert.Equal(1, i.ProductId));
        }

        [Fact]
        public async Task GetByProductId_ReturnsEmptyList()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);

            var result = await repository.GetByProductId(99);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByProductId_OrdersBySortOrder()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.ProductImages.AddRangeAsync(
                new ProductImage { Id = 1, ProductId = 1, Product = product, FileName = "img1.png", SortOrder = 2 },
                new ProductImage { Id = 2, ProductId = 1, Product = product, FileName = "img2.png", SortOrder = 0 },
                new ProductImage { Id = 3, ProductId = 1, Product = product, FileName = "img3.png", SortOrder = 1 }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetByProductId(1);
            Assert.Equal(3, result.Count);
            Assert.Equal(0, result[0].SortOrder);
            Assert.Equal(1, result[1].SortOrder);
            Assert.Equal(2, result[2].SortOrder);
        }

        [Fact]
        public async Task GetById_ReturnImage()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            var image = new ProductImage { Id = 1, ProductId = 1, Product = product, FileName = "img1.png", IsPrimary = true, SortOrder = 0 };
            await context.ProductImages.AddAsync(image);
            await context.SaveChangesAsync();

            var result = await repository.GetById(1);
            Assert.NotNull(result);
            Assert.Equal("img1.png", result.FileName);
            Assert.True(result.IsPrimary);
        }

        [Fact]
        public async Task GetById_ReturnNull()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);

            var result = await repository.GetById(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task Add_AddsImage()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var image = new ProductImage { Id = 1, ProductId = 1, Product = product, FileName = "new.png", IsPrimary = true, SortOrder = 0 };
            await repository.Add(image);

            var result = await context.ProductImages.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("new.png", result.FileName);
        }

        [Fact]
        public async Task Update_UpdatesImage()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            var image = new ProductImage { Id = 1, ProductId = 1, Product = product, FileName = "old.png", IsPrimary = false, SortOrder = 0 };
            await context.ProductImages.AddAsync(image);
            await context.SaveChangesAsync();

            image.FileName = "updated.png";
            image.IsPrimary = true;
            await repository.Update(image);

            var updated = await context.ProductImages.FindAsync(1);
            Assert.Equal("updated.png", updated.FileName);
            Assert.True(updated.IsPrimary);
        }

        [Fact]
        public async Task Delete_DeletesImage()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            var image = new ProductImage { Id = 1, ProductId = 1, Product = product, FileName = "img.png", IsPrimary = true, SortOrder = 0 };
            await context.ProductImages.AddAsync(image);
            await context.SaveChangesAsync();

            await repository.Delete(image);
            var deleted = await context.ProductImages.FindAsync(1);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetCountByProductId_ReturnsCorrectCount()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Products.AddAsync(product);
            await context.ProductImages.AddRangeAsync(
                new ProductImage { Id = 1, ProductId = 1, Product = product, FileName = "img1.png", SortOrder = 0 },
                new ProductImage { Id = 2, ProductId = 1, Product = product, FileName = "img2.png", SortOrder = 1 }
            );
            await context.SaveChangesAsync();

            var count = await repository.GetCountByProductId(1);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountByProductId_ReturnsZeroForUnknownProduct()
        {
            using var context = CreateContext();
            var repository = new ProductImageRepositoryEfCore(context);

            var count = await repository.GetCountByProductId(99);
            Assert.Equal(0, count);
        }
    }
}
