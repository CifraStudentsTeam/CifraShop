using CifraShop.API.Controllers;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Orders;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CifraShop.Tests.ControllerTests
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IOrderImageRepository> _imageRepositoryMock;
        private readonly OrderController _controller;

        // Инициализирует моки IOrderService и IOrderImageRepository и создаёт экземпляр OrderController перед каждым тестом.
        public OrderControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _imageRepositoryMock = new Mock<IOrderImageRepository>();
            _controller = new OrderController(_orderServiceMock.Object, _imageRepositoryMock.Object, null!, null!);
        }

        private static Order CreateOrder(int id = 1, StatusOrder status = StatusOrder.Pending, int sum = 100)
            => new() { Id = id, Status = status, Sum = sum, DateOfPurchase = DateTime.UtcNow, CustomerId = 1, Customer = new User { Id = 1, Email = "test@email.com" } };

        [Fact]
        //РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ GetAll РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK СЃРѕ СЃРїРёСЃРєРѕРј РёР· РґРІСѓС… Р·Р°РєР°Р·РѕРІ.
        public async Task CheckingGetAllReturnsOkWith2Orders()
        {
            _orderServiceMock.Setup(s => s.GetAllOrders())
                .ReturnsAsync(new List<Order> { CreateOrder(1), CreateOrder(2) });

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.Orders.OrderResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїРѕСЃС‚СЂР°РЅРёС‡РЅРѕРµ РїРѕР»СѓС‡РµРЅРёРµ Р·Р°РєР°Р·РѕРІ (СЃС‚СЂР°РЅРёС†Р° 0, СЂР°Р·РјРµСЂ 10) РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK СЃ РЅРµРїСѓСЃС‚С‹Рј С‚РµР»РѕРј
        public async Task CheckingPagedOrdersReturnsOk()
        {
            var paged = new Contracts.Responses.Common.PagedResponse<Order>
            {
                Items = new List<Order> { CreateOrder() },
                Page = 0, PageSize = 10, TotalCount = 1
            };
            _orderServiceMock.Setup(s => s.GetOrdersPaged(0, 10, null, null, null, null)).ReturnsAsync(paged);

            var result = await _controller.GetPaged(0, 10);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РЅР°С…РѕР¶РґРµРЅРёРё Р·Р°РєР°Р·Р° РїРѕ ID РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 200 OK СЃ РЅРµРїСѓСЃС‚С‹Рј С‚РµР»РѕРј
        public async Task CheckingFoundOrderReturnsOKWithNonEmptyBody()
        {
            _orderServiceMock.Setup(s => s.GetOrderById(1)).ReturnsAsync(CreateOrder(1));

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё Р·Р°РєР°Р·Р° СЃ ID 99 РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 404 Not Found
        public async Task CheckingMissingOrderReturnsNotFound()
        {
            _orderServiceMock.Setup(s => s.GetOrderById(99)).ReturnsAsync((Order?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    
        [Fact]
        //РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїРѕР»СѓС‡РµРЅРёРµ Р·Р°РєР°Р·РѕРІ РїРѕ email РєР»РёРµРЅС‚Р° РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK СЃ РЅРµРїСѓСЃС‚С‹Рј С‚РµР»РѕРј
        public async Task CheckingOrdersByEmailReturnsOk()
        {
            _orderServiceMock.Setup(s => s.GetOrdersByCustomerEmail("a@b.com"))
                .ReturnsAsync(new List<Order> { CreateOrder() });

            var result = await _controller.GetByCustomerEmail("a@b.com");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїРѕР»СѓС‡РµРЅРёРµ Р·Р°РєР°Р·РѕРІ РїРѕ СЃС‚Р°С‚СѓСЃСѓ Completed РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK СЃ РЅРµРїСѓСЃС‚С‹Рј С‚РµР»РѕРј.
        public async Task CheckingStatusCompletedReturnsOk()
        {
            _orderServiceMock.Setup(s => s.GetOrdersByStatus(StatusOrder.Completed))
                .ReturnsAsync(new List<Order> { CreateOrder(1, StatusOrder.Completed) });

            var result = await _controller.GetByStatus(StatusOrder.Completed);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        //
        public async Task GetBySum_ReturnsOk()
        {
            _orderServiceMock.Setup(s => s.GetOrdersBySum(100))
                .ReturnsAsync(new List<Order> { CreateOrder() });

            var result = await _controller.GetBySum(100);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetByDate_ReturnsOk()
        {
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 12, 31);
            _orderServiceMock.Setup(s => s.GetOrdersByDateRange(from, to))
                .ReturnsAsync(new List<Order> { CreateOrder() });

            var result = await _controller.GetByDate(from, to);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Create_ValidData_ReturnsCreatedAtAction()
        {
            var order = CreateOrder();
            _orderServiceMock.Setup(s => s.CreateOrder("a@b.com", It.IsAny<List<(int, int)>>()))
                .ReturnsAsync(order);

            var result = await _controller.Create(new CreateOrderRequest
            {
                CustomerEmail = "a@b.com",
                Items = new List<Contracts.Requests.OrderItem.OrderItemRequest>
                {
                    new() { ProductId = 1, Quantity = 2 }
                }
            });

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(OrderController.GetById), createdResult.ActionName);
        }

        [Fact]
        public async Task Update_Found_ReturnsNoContent()
        {
            var order = CreateOrder();
            _orderServiceMock.Setup(s => s.GetOrderById(1)).ReturnsAsync(order);
            _orderServiceMock.Setup(s => s.UpdateOrder(It.IsAny<Order>())).Returns(Task.CompletedTask);

            var result = await _controller.Update(1, new UpdateOrderRequest { Status = StatusOrder.Paid });

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(StatusOrder.Paid, order.Status);
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            _orderServiceMock.Setup(s => s.GetOrderById(99)).ReturnsAsync((Order?)null);

            var result = await _controller.Update(99, new UpdateOrderRequest { Status = StatusOrder.Paid });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task BatchUpdateStatus_ValidIds_ReturnsOk()
        {
            _orderServiceMock.Setup(s => s.UpdateStatusRange(It.IsAny<List<int>>(), StatusOrder.Paid))
                .Returns(Task.CompletedTask);

            var result = await _controller.BatchUpdateStatus(new BatchUpdateOrderStatusRequest
            {
                OrderIds = new List<int> { 1, 2 }, NewStatus = StatusOrder.Paid
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task BatchUpdateStatus_EmptyIds_ReturnsBadRequest()
        {
            var result = await _controller.BatchUpdateStatus(new BatchUpdateOrderStatusRequest { OrderIds = new List<int>() });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BatchUpdateStatus_NullIds_ReturnsBadRequest()
        {
            var result = await _controller.BatchUpdateStatus(new BatchUpdateOrderStatusRequest { OrderIds = null! });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Found_ReturnsNoContent()
        {
            var order = CreateOrder();
            _orderServiceMock.Setup(s => s.GetOrderById(1)).ReturnsAsync(order);
            _orderServiceMock.Setup(s => s.DeleteOrder(order)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _orderServiceMock.Setup(s => s.GetOrderById(99)).ReturnsAsync((Order?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
