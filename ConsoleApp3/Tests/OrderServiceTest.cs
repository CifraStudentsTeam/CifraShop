using ConsoleApp3.Models;
using ConsoleApp3.Servise;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleApp3.Tests
{
    public class OrderServiceTest
    {
        private async Task<AplicationContext> GetDataBaseContext()
        {
            var options = new DbContextOptionsBuilder<AplicationContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var dbContext = new AplicationContext(options);
            await dbContext.Database.EnsureCreatedAsync();
            return dbContext;
        }

        [Fact]
        public async Task CreateOrder_AddsOrder()
        {
            var context = await GetDataBaseContext();
            var service = new OrderService(context);

            var order = await service.CreateOrder(StatusOrder.AwaitingPayment, 100, "customer1");

            Assert.NotNull(order);
            Assert.Equal("customer1", order.CustomerLogin);
            Assert.Equal((uint)100, order.Sum);
            Assert.Single(context.Orders);
        }

        [Fact]
        public async Task GetOrdersByLogin_ReturnsFiteredOrders()
        {
            var context = await GetDataBaseContext();
            var service = new OrderService(context);
            context.Orders.AddRange
            (
                new Order { CustomerLogin = "cust1", Sum = 10 },
                new Order { CustomerLogin = "cust2", Sum = 20 },
                new Order { CustomerLogin = "cust3", Sum = 30 }
            );
            await context.SaveChangesAsync();

            var orders = await service.GetOrdersByLogin("cust1");

            Assert.Equal(2, orders.Count);
            Assert.All(orders, o => Assert.Equal("cust1", o.CustomerLogin));
        }

        [Fact]
        public async Task ChangeOrderStatus_UpdateStatus()
        {
            var context = await GetDataBaseContext();
            var service = new OrderService(context);
            var order = new Order { Status = StatusOrder.AwaitingPayment };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var updated = await service.ChangeOrderStatus(order, StatusOrder.Completed);

            Assert.Equal(StatusOrder.Completed, updated.Status);
            var dbOrder = await context.Orders.FindAsync(order.Id);
            Assert.Equal(StatusOrder.Completed, dbOrder.Status);
        }
       
 
    }
}
