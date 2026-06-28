using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CifraShop.Tests.RepositoryTests
{
    public class OrderItemRepositoryTests
    {
        private ApplicationContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationContext(options);
        }

        [Fact]
        //Тест с валидными данными получения состовляющих заказа по id заказа
        public async Task GetOrderItemsByOrderId_ReturnOrderItemsForGivenOrderId()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 250, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
            var orderOther = new Order { Id = 2, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
            var product = new Product { Id = 1, Name = "Кружка", Description = "Кружка с логотипом цифры", Price = 100, Quantity = 1, Status = StatusProduct.InStock };
            var orderItem1 = new OrderItem { Id = 1, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };
            var orderItem2 = new OrderItem { Id = 2, Price = 100, Quantity = 1, OrderId= order.Id, Order = order, ProductId= product.Id, Product = product };
            var orderItemOther = new OrderItem { Id = 3, Price = 100, Quantity = 1, OrderId = orderOther.Id, Order= orderOther, ProductId= product.Id, Product = product };

            await context.Users.AddAsync(customer);
            await context.Orders.AddRangeAsync(order, orderOther);
            await context.Products.AddAsync(product);
            await context.OrderItems.AddRangeAsync(orderItem1, orderItem2, orderItemOther);
            await context.SaveChangesAsync();

            var result = await repository.GetOrderItemsByOrderId(order.Id);
            Assert.Equal(2, result.Count);
            Assert.All(result, oi => Assert.Equal(1, oi.OrderId));
            Assert.Contains(result, oi => oi.Id == 1);
            Assert.Contains(result, oi => oi.Id == 2);
        }

        [Fact]
        //Тест с невалидными данными получения состовляющих заказа по id заказа
        public async Task GetOrderItemsByOrderId_ReturnsEmptyListWhenNoItems()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var result = await repository.GetOrderItemsByOrderId(99);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        //Тест с валидными данными получения состовляющей заказа по id
        public async Task GetOrderItemsById_ReturnOrderItemWhenExists()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 250, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
            var product = new Product { Id = 1, Name = "Кружка", Description = "Кружка с логотипом цифры", Price = 100, Quantity = 1, Status = StatusProduct.InStock };
            var orderItem1 = new OrderItem { Id = 1, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };
            var orderItem2 = new OrderItem { Id = 2, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };

            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.Products.AddAsync(product);
            await context.OrderItems.AddRangeAsync(orderItem1, orderItem2);
            await context.SaveChangesAsync();

            var result = await repository.GetOrderItemById(orderItem1.Id);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.OrderId);
            Assert.Equal(1, result.ProductId);
            Assert.NotNull(result.Order);
            Assert.NotNull(result.Product);
        }

        [Fact]
        // Тест с невалидными данными получения состовляющей заказа по id
        public async Task GetOrderItemById_ReturnOrderItemWhenNoExists()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var result = await repository.GetOrderItemById(99);
            Assert.Null(result);
        }

        [Fact]
        //Тест добавления состовляющей заказа 
        public async Task AddOrderItem_AddOrderItem()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 250, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
            var product = new Product { Id = 1, Name = "Кружка", Description = "Кружка с логотипом цифры", Price = 100, Quantity = 1, Status = StatusProduct.InStock };
            var orderItem1 = new OrderItem { Id = 1, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };
            var orderItem2 = new OrderItem { Id = 2, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };

            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            await repository.AddOrderItem(orderItem1);

            var result = await context.OrderItems.FirstOrDefaultAsync(x => x.Id == orderItem1.Id);
            Assert.NotNull(result);
            Assert.Equal(result.Id, 1);
        }

        [Fact]
        //Тест обновления состовляющей заказа
        public async Task UpdateOrderItem_UpdatesOrderItem()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 250, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
            var product = new Product { Id = 1, Name = "Кружка", Description = "Кружка с логотипом цифры", Price = 100, Quantity = 1, Status = StatusProduct.InStock };
            var product2 = new Product { Id = 2, Name = "Брелок", Description = "Брелок с логотипом цифры", Price = 50, Quantity = 1, Status = StatusProduct.InStock };
            var orderItem1 = new OrderItem { Id = 1, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };
            var orderItem2 = new OrderItem { Id = 2, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };

            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.Products.AddRangeAsync(product, product2);
            await context.OrderItems.AddRangeAsync(orderItem1, orderItem2);
            await context.SaveChangesAsync();
            orderItem1.ProductId = product2.Id;
            orderItem1.Product = product;
            await repository.UpdateOrderItem(orderItem1);
            var updatedItem = await context.OrderItems.FindAsync(1);
            Assert.Equal(2, updatedItem.ProductId);
        }

        [Fact]
        //Тест удаления обновления состовляющей заказа
        public async Task DeleteOrderItem_RemovesOrderItem()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 250, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
            var product = new Product { Id = 1, Name = "Кружка", Description = "Кружка с логотипом цифры", Price = 100, Quantity = 1, Status = StatusProduct.InStock };
            var orderItem1 = new OrderItem { Id = 1, Price = 100, Quantity = 1, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };
            var orderItem2 = new OrderItem { Id = 2, Price = 100, Quantity = 2, OrderId = order.Id, Order = order, ProductId = product.Id, Product = product };

            await context.Users.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.Products.AddAsync(product);
            await context.OrderItems.AddRangeAsync(orderItem1, orderItem2);
            await context.SaveChangesAsync();
            await repository.DeleteOrderItem(orderItem1);
            var deletedItem = await context.OrderItems.FindAsync(1);
            Assert.Null(deletedItem);
        }
    }
}
                        