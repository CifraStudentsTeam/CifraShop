using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data;
using CifraShop.Infrastructure.Data.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public async Task GetOrderItemsByOrderId_ReturnOrderItemsForGivenOrderId()
        {
            using var context = CreateContext();
            var repository = new OrderItemRepositoryEfCore(context);
            var customer = new User { Id = 1, Email = "testuser@email.com", Password = "12345678", Balance = 250, Role = UserRole.Student };
            var order = new Order { Id = 1, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
            var orderOther = new Order { Id = 2, Sum = 100, Status = StatusOrder.AwaitingPayment, DateOfPurchase = DateTime.UtcNow, CustomerId = customer.Id, Customer = customer };
        }
    }
}
                        