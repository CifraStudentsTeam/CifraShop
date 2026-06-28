using CifraShop.API.Controllers;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.OrderItem;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CifraShop.Tests.ControllerTests
{
    public class OrderItemControllerTests
    {
        private readonly Mock<IOrderItemService> _serviceMock;
        private readonly OrderItemController _controller;

        public OrderItemControllerTests()
        {
            _serviceMock = new Mock<IOrderItemService>();
            _controller = new OrderItemController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetOrderItemById_Found_ReturnsOk()
        {
            var item = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, Price = 100 };
            _serviceMock.Setup(s => s.GetOrderItemById(1)).ReturnsAsync(item);

            var result = await _controller.GetOrderItemById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetOrderItemById_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetOrderItemById(99)).ReturnsAsync((OrderItem?)null);

            var result = await _controller.GetOrderItemById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetOrderItemsByOrderId_ReturnsOk()
        {
            var items = new List<OrderItem>
            {
                new() { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, Price = 100 },
                new() { Id = 2, OrderId = 1, ProductId = 2, Quantity = 3, Price = 50 }
            };
            _serviceMock.Setup(s => s.GetOrderItemsByOrderId(1)).ReturnsAsync(items);

            var result = await _controller.GetOrderItemsByOrderId(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.OrderItem.OrderItemResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task CreateOrderItem_ValidData_ReturnsCreatedAtAction()
        {
            var item = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 2, Price = 100 };
            _serviceMock.Setup(s => s.CreateOrderItem(1, 1, 2)).ReturnsAsync(item);

            var result = await _controller.CreateOrderItem(new CreateOrderItemRequest
            {
                OrderId = 1, ProductId = 1, Quantity = 2
            });

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(OrderItemController.GetOrderItemById), createdResult.ActionName);
        }

        [Fact]
        public async Task UpdateOrderItem_Found_ReturnsNoContent()
        {
            var existing = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, Price = 100 };
            _serviceMock.Setup(s => s.GetOrderItemById(1)).ReturnsAsync(existing);
            _serviceMock.Setup(s => s.UpdateOrderItem(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrderItem(1, new UpdateOrderItemRequest { Id = 1, Quantity = 5 });

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(5, existing.Quantity);
        }

        [Fact]
        public async Task UpdateOrderItem_IdMismatch_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOrderItem(1, new UpdateOrderItemRequest { Id = 2, Quantity = 5 });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateOrderItem_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetOrderItemById(99)).ReturnsAsync((OrderItem?)null);

            var result = await _controller.UpdateOrderItem(99, new UpdateOrderItemRequest { Id = 99, Quantity = 5 });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateOrderItem_UpdatesPrice()
        {
            var existing = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, Price = 100 };
            _serviceMock.Setup(s => s.GetOrderItemById(1)).ReturnsAsync(existing);
            _serviceMock.Setup(s => s.UpdateOrderItem(It.IsAny<OrderItem>())).Returns(Task.CompletedTask);

            await _controller.UpdateOrderItem(1, new UpdateOrderItemRequest { Id = 1, Quantity = 1, Price = 200 });

            Assert.Equal(200, existing.Price);
        }

        [Fact]
        public async Task DeleteOrderItem_Found_ReturnsNoContent()
        {
            var existing = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, Price = 100 };
            _serviceMock.Setup(s => s.GetOrderItemById(1)).ReturnsAsync(existing);
            _serviceMock.Setup(s => s.DeleteOrderItem(existing)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteOrderItem(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOrderItem_NotFound_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetOrderItemById(99)).ReturnsAsync((OrderItem?)null);

            var result = await _controller.DeleteOrderItem(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
