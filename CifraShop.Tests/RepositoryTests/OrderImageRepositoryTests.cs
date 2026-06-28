using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Tests.RepositoryTests
{
    public class OrderImageRepositoryTests
    {
        private ApplicationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationContext(options);
        }

        [Fact]
        public async Task GetByOrderId_ReturnsImagesForOrder()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            var order2 = new Order { Id = 2, Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(order, order2);
            await context.OrderImages.AddRangeAsync(
                new OrderImage { Id = 1, OrderId = 1, Order = order, FileName = "receipt1.png", IsPrimary = true, SortOrder = 0 },
                new OrderImage { Id = 2, OrderId = 1, Order = order, FileName = "receipt2.png", IsPrimary = false, SortOrder = 1 },
                new OrderImage { Id = 3, OrderId = 2, Order = order2, FileName = "receipt3.png", IsPrimary = true, SortOrder = 0 }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetByOrderId(1);
            Assert.Equal(2, result.Count);
            Assert.All(result, i => Assert.Equal(1, i.OrderId));
        }

        [Fact]
        public async Task GetByOrderId_ReturnsEmptyList()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);

            var result = await repository.GetByOrderId(99);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByOrderId_OrdersBySortOrder()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.OrderImages.AddRangeAsync(
                new OrderImage { Id = 1, OrderId = 1, Order = order, FileName = "img1.png", SortOrder = 2 },
                new OrderImage { Id = 2, OrderId = 1, Order = order, FileName = "img2.png", SortOrder = 0 },
                new OrderImage { Id = 3, OrderId = 1, Order = order, FileName = "img3.png", SortOrder = 1 }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetByOrderId(1);
            Assert.Equal(0, result[0].SortOrder);
            Assert.Equal(1, result[1].SortOrder);
            Assert.Equal(2, result[2].SortOrder);
        }

        [Fact]
        public async Task GetById_ReturnImage()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.OrderImages.AddAsync(new OrderImage { Id = 1, OrderId = 1, Order = order, FileName = "receipt.png", IsPrimary = true, SortOrder = 0 });
            await context.SaveChangesAsync();

            var result = await repository.GetById(1);
            Assert.NotNull(result);
            Assert.Equal("receipt.png", result.FileName);
        }

        [Fact]
        public async Task GetById_ReturnNull()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);

            var result = await repository.GetById(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task Add_AddsImage()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            var image = new OrderImage { Id = 1, OrderId = 1, Order = order, FileName = "new.png", IsPrimary = true, SortOrder = 0 };
            await repository.Add(image);

            var result = await context.OrderImages.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal("new.png", result.FileName);
        }

        [Fact]
        public async Task Update_UpdatesImage()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            var image = new OrderImage { Id = 1, OrderId = 1, Order = order, FileName = "old.png", IsPrimary = false, SortOrder = 0 };
            await context.OrderImages.AddAsync(image);
            await context.SaveChangesAsync();

            image.FileName = "updated.png";
            image.IsPrimary = true;
            await repository.Update(image);

            var updated = await context.OrderImages.FindAsync(1);
            Assert.Equal("updated.png", updated.FileName);
            Assert.True(updated.IsPrimary);
        }

        [Fact]
        public async Task Delete_DeletesImage()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            var image = new OrderImage { Id = 1, OrderId = 1, Order = order, FileName = "img.png", IsPrimary = true, SortOrder = 0 };
            await context.OrderImages.AddAsync(image);
            await context.SaveChangesAsync();

            await repository.Delete(image);
            var deleted = await context.OrderImages.FindAsync(1);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetCountByOrderId_ReturnsCorrectCount()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.OrderImages.AddRangeAsync(
                new OrderImage { Id = 1, OrderId = 1, Order = order, FileName = "img1.png", SortOrder = 0 },
                new OrderImage { Id = 2, OrderId = 1, Order = order, FileName = "img2.png", SortOrder = 1 }
            );
            await context.SaveChangesAsync();

            var count = await repository.GetCountByOrderId(1);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountByOrderId_ReturnsZeroForUnknownOrder()
        {
            using var context = CreateContext();
            var repository = new OrderImageRepositoryEfCore(context);

            var count = await repository.GetCountByOrderId(99);
            Assert.Equal(0, count);
        }
    }
}
