using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CifraShop.Tests.RepositoryTests
{
    public class OrderRepositoryTests
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
        public async Task GetAllOrders_ReturnOrders()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order1 = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            var order2 = new Order { Id = 2, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            var order3 = new Order { Id = 3, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            await context.AddRangeAsync(order1, order2, order3);
            await context.SaveChangesAsync();
            var result = await repository.GetAll();
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllOrders_ReturnsEmptyList()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);

            var result = await repository.GetAll();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrderById_ReturnOrder()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order1 = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            var order2 = new Order { Id = 2, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            var order3 = new Order { Id = 3, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            await context.AddRangeAsync(order1, order2, order3);
            await context.SaveChangesAsync();
            var result = await repository.GetOrderById(order1.Id);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetOrderById_ReturnNull()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var result = await repository.GetOrderById(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrdersByCustomerEmail_ReturnOrders()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var student = new User { Id = 2, Email = "teststudent@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var order1 = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            var order2 = new Order { Id = 2, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = customer.Id, Customer = customer };
            var order3 = new Order { Id = 3, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = student.Id, Customer = student };
            await context.Users.AddRangeAsync(customer, student);
            await context.Orders.AddRangeAsync(order1, order2, order3);
            await context.SaveChangesAsync();

            var result = await repository.GetOrdersByCustomerEmail(customer.Email);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllPaged_ReturnsPagedResult()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            for (int i = 1; i <= 5; i++)
            {
                await context.Orders.AddAsync(new Order { Id = i, Sum = i * 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer });
            }
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllPaged(0, 2);
            Assert.Equal(2, items.Count);
            Assert.Equal(5, totalCount);
        }

        [Fact]
        public async Task GetAllPaged_SearchById()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 2, Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer }
            );
            await context.SaveChangesAsync();

            var (items, _) = await repository.GetAllPaged(0, 10, search: "1");
            Assert.Single(items);
            Assert.Equal(1, items[0].Id);
        }

        [Fact]
        public async Task GetAllPaged_SearchByEmail()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer1 = new User { Id = 1, Email = "alice@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var customer2 = new User { Id = 2, Email = "bob@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddRangeAsync(customer1, customer2);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer1 },
                new Order { Id = 2, Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 2, Customer = customer2 }
            );
            await context.SaveChangesAsync();

            var (items, _) = await repository.GetAllPaged(0, 10, search: "alice");
            Assert.Single(items);
            Assert.Equal(1, items[0].CustomerId);
        }

        [Fact]
        public async Task GetAllPaged_FilterByStatus()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 2, Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer }
            );
            await context.SaveChangesAsync();

            var (items, totalCount) = await repository.GetAllPaged(0, 10, status: StatusOrder.Pending);
            Assert.Single(items);
            Assert.Equal(StatusOrder.Pending, items[0].Status);
        }

        [Fact]
        public async Task GetAllPaged_FilterByDateRange()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = new DateTime(2025, 1, 15), Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 2, Sum = 200, DateOfPurchase = new DateTime(2025, 6, 15), Status = StatusOrder.Completed, CustomerId = 1, Customer = customer }
            );
            await context.SaveChangesAsync();

            var (items, _) = await repository.GetAllPaged(0, 10, dateFrom: new DateTime(2025, 6, 1), dateTo: new DateTime(2025, 12, 31));
            Assert.Single(items);
            Assert.Equal(2, items[0].Id);
        }

        [Fact]
        public async Task GetOrdersBySum_ReturnOrders()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 2, Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 3, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetOrdersBySum(100);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetOrdersByStatus_ReturnOrders()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 2, Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer },
                new Order { Id = 3, Sum = 300, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetOrdersByStatus(StatusOrder.Completed);
            Assert.Equal(2, result.Count);
            Assert.All(result, o => Assert.Equal(StatusOrder.Completed, o.Status));
        }

        [Fact]
        public async Task GetOrdersByDateRange_ReturnOrders()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = new DateTime(2025, 1, 15), Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 2, Sum = 200, DateOfPurchase = new DateTime(2025, 6, 15), Status = StatusOrder.Completed, CustomerId = 1, Customer = customer },
                new Order { Id = 3, Sum = 300, DateOfPurchase = new DateTime(2025, 12, 15), Status = StatusOrder.Completed, CustomerId = 1, Customer = customer }
            );
            await context.SaveChangesAsync();

            var result = await repository.GetOrdersByDateRange(new DateTime(2025, 3, 1), new DateTime(2025, 9, 30));
            Assert.Single(result);
            Assert.Equal(2, result[0].Id);
        }

        [Fact]
        public async Task AddOrder_AddsOrder()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.SaveChangesAsync();

            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer };
            await repository.AddOrder(order);

            var result = await context.Orders.FindAsync(1);
            Assert.NotNull(result);
            Assert.Equal(100, result.Sum);
            Assert.Equal(StatusOrder.Pending, result.Status);
        }

        [Fact]
        public async Task UpdateOrder_UpdatesOrder()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer };
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            order.Status = StatusOrder.Paid;
            order.Sum = 250;
            await repository.UpdateOrder(order);

            var updated = await context.Orders.FindAsync(1);
            Assert.Equal(StatusOrder.Paid, updated.Status);
            Assert.Equal(250, updated.Sum);
        }

        [Fact(Skip = "ExecuteUpdateAsync РЅРµ РїРѕРґРґРµСЂР¶РёРІР°РµС‚СЃСЏ InMemory-РїСЂРѕРІР°Р№РґРµСЂРѕРј. РўРµСЃС‚ С‚СЂРµР±СѓРµС‚ СЂРµР°Р»СЊРЅСѓСЋ Р‘Р” (SQL Server).")]
        public async Task UpdateStatusRange_UpdatesMultipleOrders()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(
                new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer },
                new Order { Id = 2, Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer },
                new Order { Id = 3, Sum = 300, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer }
            );
            await context.SaveChangesAsync();

            await repository.UpdateStatusRange(new List<int> { 1, 3 }, StatusOrder.Paid);

            var o1 = await context.Orders.FindAsync(1);
            var o2 = await context.Orders.FindAsync(2);
            var o3 = await context.Orders.FindAsync(3);
            Assert.Equal(StatusOrder.Paid, o1.Status);
            Assert.Equal(StatusOrder.Pending, o2.Status);
            Assert.Equal(StatusOrder.Paid, o3.Status);
        }

        [Fact]
        public async Task DeleteOrder_DeletesOrder()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            await context.Users.AddAsync(customer);
            var order = new Order { Id = 1, Sum = 100, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Completed, CustomerId = 1, Customer = customer };
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            await repository.DeleteOrder(order);
            var deleted = await context.Orders.FindAsync(1);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task CreateOrderInTransaction_CreatesOrderWithItemsAndUpdatesStock()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var product = new Product { Id = 1, Name = "Кружка", Description = "Описание", Price = 100, Quantity = 5, Status = StatusProduct.InStock };
            await context.Users.AddAsync(customer);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

        //    var order = new Order { Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer };
        //    var items = new List<OrderItem>
        //    {
        //        new OrderItem { Price = 100, Quantity = 2, ProductId = 1, Product = product }
        //    };
        //    var stockUpdates = new List<(int ProductId, int Quantity)> { (1, 2) };

            var result = await repository.CreateOrderInTransaction(order, items, stockUpdates, 1, 100);

        //    Assert.NotNull(result);
        //    Assert.True(result.Id > 0);

        //    var savedOrder = await context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == result.Id);
        //    Assert.NotNull(savedOrder);
        //    Assert.Single(savedOrder.OrderItems);
        //    Assert.Equal(200, savedOrder.Sum);

        //    var updatedProduct = await context.Products.FindAsync(1);
        //    Assert.Equal(3, updatedProduct.Quantity);
        //    Assert.Equal(StatusProduct.InStock, updatedProduct.Status);
        //}

        [Fact]
        public async Task CreateOrderInTransaction_SetsOutOfStockWhenQuantityReachesZero()
        {
            using var context = CreateContext();
            var repository = new OrderRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "test@email.com", Password = "12345678", Balance = 0, Role = UserRole.Student };
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 2, Status = StatusProduct.InStock };
            await context.Users.AddAsync(customer);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            var order = new Order { Sum = 200, DateOfPurchase = DateTime.UtcNow, Status = StatusOrder.Pending, CustomerId = 1, Customer = customer };
            var items = new List<OrderItem>
            {
                new OrderItem { Price = 100, Quantity = 2, ProductId = 1, Product = product }
            };
            var stockUpdates = new List<(int ProductId, int Quantity)> { (1, 2) };

            await repository.CreateOrderInTransaction(order, items, stockUpdates, 1, 100);

            var updatedProduct = await context.Products.FindAsync(1);
            Assert.Equal(0, updatedProduct.Quantity);
            Assert.Equal(StatusProduct.OutOfStock, updatedProduct.Status);
        }
    }
}
