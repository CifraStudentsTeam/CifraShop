using CifraShopLiblary.Data.DataForModels;
using CifraShopLiblary.Models;
using CifraShopTest.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace CifraShopTest
{
    public class OrderServiceTest : OrederServiceTestBase
    {
        [Fact]
        public async Task CreateOrder_ValidData_ReturnOrder()
        {
            var status = StatusOrder.AwaitingPayment;
            uint sum = 2000;
            var login = "createlogin";
            var result = await _service.CreateOrder(status, sum, login);
            Assert.NotNull(result);
            Assert.Equal(status, result.Status);
            Assert.Equal(sum, result.Sum);
            Assert.Equal(login, result.CustomerLogin);
        }

        [Fact]
        public async Task CreateOrder_AddToDatabase()
        {
            var login = "databaselogin";
            await _service.CreateOrder(StatusOrder.PaidFor, 100, login);
            var result = await _context.Orders.FirstOrDefaultAsync(x => x.CustomerLogin == login);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetOrderById_ExssistingId_ReturnOrder()
        {
            var order = new Order
            {
                Status = StatusOrder.AwaitingPayment,
                Sum = 100,
                CustomerLogin = "IdLogin",
                DateOfPurchase = DateTime.Now
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            var result = await _service.GetOrderById(order.Id);
            Assert.NotNull(result);
            Assert.Equal("IdLogin", result.CustomerLogin);
        }

        [Fact]
        public async Task GetOrderById_NonExssistingId_ReturnNull()
        {
            var result = await _service.GetOrderById(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrdesByLogin_ExssistingLogin_ReturnOrders()
        {
            var login = "logins";

            await _context.AddRangeAsync(
                new Order { Status = StatusOrder.AwaitingPayment, CustomerLogin = login, Sum = 100, DateOfPurchase = DateTime.Now },
                new Order { Status = StatusOrder.AwaitingPayment, CustomerLogin = login, Sum = 100, DateOfPurchase = DateTime.Now },
                new Order { Status = StatusOrder.AwaitingPayment, CustomerLogin = "nologins", Sum = 100, DateOfPurchase = DateTime.Now }
            );

            await _context.SaveChangesAsync();
            var result = await _service.GetOrdersByLogin(login);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(login, x.CustomerLogin));
        }

        [Fact]
        public async Task GetOrdersByStatus_ReturnOrders()
        {
            var status = StatusOrder.Completed;

            await _context.AddRangeAsync(
                new Order { Status = status, CustomerLogin = "statuslogin", Sum = 100, DateOfPurchase = DateTime.Now },
                new Order { Status = status, CustomerLogin = "statuslogin", Sum = 100, DateOfPurchase = DateTime.Now },
                new Order { Status = StatusOrder.AwaitingPayment, CustomerLogin  = "statuslogin", DateOfPurchase = DateTime.Now}
            );

            await _context.SaveChangesAsync();
            var result = await _service.GetOrdersByStatus(status);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(status, x.Status));
        }

        [Fact]
        public async Task GetOrdersBySum_ReturnOrders()
        {
             uint sum = 200;

            await _context.AddRangeAsync(
                new Order { Status = StatusOrder.AwaitingPayment, CustomerLogin = "sumlogin", Sum = sum, DateOfPurchase = DateTime.Now },
                new Order { Status = StatusOrder.AwaitingPayment, CustomerLogin = "sumlogin", Sum = sum, DateOfPurchase = DateTime.Now },
                new Order { Status = StatusOrder.AwaitingPayment, CustomerLogin = "sumlogin", Sum = 100, DateOfPurchase = DateTime.Now }
            );

            await _context.SaveChangesAsync();
            var result = await _service.GetOrdersBySum(sum);
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal(sum, x.Sum));
        }

        [Fact]
        public async Task ChangeOrderStatus_ReturnOrder()
        {
            var order = new Order
            {
                Status = StatusOrder.AwaitingPayment,
                CustomerLogin = "changestatus",
                Sum = 100,
                DateOfPurchase = DateTime.Now
            };

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();
            var newStatus = StatusOrder.ManufacturedBy;
            var updatedOrder = await _service.ChangeOrderStatus(order, newStatus);
            Assert.NotNull(updatedOrder);
            Assert.Equal(newStatus, updatedOrder.Status);
        }

        [Fact]
        public async Task DeleteOrder_FromDatabase()
        {
            var oderToDelete = new Order
            {
                Status = StatusOrder.AwaitingPayment,
                CustomerLogin = "deletelogin",
                Sum = 100,
                DateOfPurchase = DateTime.Now
            };

            await _context.Orders.AddAsync(oderToDelete);
            await _context.SaveChangesAsync();
            await _service.DeleteOrder(oderToDelete);
            var deletedOrder = await _context.Orders.FindAsync(oderToDelete.Id);
            Assert.Null(deletedOrder);
        }
    }
}
