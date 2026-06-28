using CifraShop.Application.Services.Implementations;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Moq;

namespace CifraShop.Tests.ServiceTests
{
    public class OrderItemServiceTests
    {
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly OrderItemService _service;

        public OrderItemServiceTests()
        {
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _service = new OrderItemService(
                _orderItemRepositoryMock.Object,
                _productRepositoryMock.Object,
                _orderRepositoryMock.Object);
        }

        [Fact]
        public async Task GetOrderItemById_ReturnsItem()
        {
            var item = new OrderItem { Id = 1, OrderId = 1, ProductId = 1 };
            _orderItemRepositoryMock.Setup(r => r.GetOrderItemById(1)).ReturnsAsync(item);

            var result = await _service.GetOrderItemById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetOrderItemById_ReturnsNull()
        {
            _orderItemRepositoryMock.Setup(r => r.GetOrderItemById(99))
                .ReturnsAsync((OrderItem?)null);

            var result = await _service.GetOrderItemById(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrderItemsByOrderId_ReturnsItems()
        {
            var items = new List<OrderItem> { new() { Id = 1, OrderId = 1 } };
            _orderItemRepositoryMock.Setup(r => r.GetOrderItemsByOrderId(1)).ReturnsAsync(items);

            var result = await _service.GetOrderItemsByOrderId(1);

            Assert.Single(result);
        }

        [Fact]
        public async Task CreateOrderItem_ValidData_CreatesItem()
        {
            var order = new Order { Id = 1 };
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Price = 100, Quantity = 5 };

            _orderRepositoryMock.Setup(r => r.GetOrderById(1)).ReturnsAsync(order);
            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);
            _orderItemRepositoryMock.Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .Returns(Task.CompletedTask);
            _productRepositoryMock.Setup(r => r.UpdateProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateOrderItem(1, 1, 2);

            Assert.Equal(1, result.OrderId);
            Assert.Equal(1, result.ProductId);
            Assert.Equal(2, result.Quantity);
            Assert.Equal(100, result.Price);
            Assert.Equal(3, product.Quantity);
            _orderItemRepositoryMock.Verify(r => r.AddOrderItem(It.IsAny<OrderItem>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderItem_ZeroQuantity_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrderItem(1, 1, 0));
        }

        [Fact]
        public async Task CreateOrderItem_NegativeQuantity_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrderItem(1, 1, -1));
        }

        [Fact]
        public async Task CreateOrderItem_OrderNotFound_Throws()
        {
            _orderRepositoryMock.Setup(r => r.GetOrderById(99))
                .ReturnsAsync((Order?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateOrderItem(99, 1, 1));
        }

        [Fact]
        public async Task CreateOrderItem_ProductNotFound_Throws()
        {
            var order = new Order { Id = 1 };
            _orderRepositoryMock.Setup(r => r.GetOrderById(1)).ReturnsAsync(order);
            _productRepositoryMock.Setup(r => r.GetProductById(99))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateOrderItem(1, 99, 1));
        }

        [Fact]
        public async Task CreateOrderItem_InsufficientStock_Throws()
        {
            var order = new Order { Id = 1 };
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Quantity = 2 };
            _orderRepositoryMock.Setup(r => r.GetOrderById(1)).ReturnsAsync(order);
            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateOrderItem(1, 1, 5));
        }

        [Fact]
        public async Task CreateOrderItem_ExactStock_SetsOutOfStock()
        {
            var order = new Order { Id = 1 };
            var product = new Product { Id = 1, Name = "РљСЂСѓР¶РєР°", Price = 100, Quantity = 3 };
            _orderRepositoryMock.Setup(r => r.GetOrderById(1)).ReturnsAsync(order);
            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);
            _orderItemRepositoryMock.Setup(r => r.AddOrderItem(It.IsAny<OrderItem>()))
                .Returns(Task.CompletedTask);
            _productRepositoryMock.Setup(r => r.UpdateProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            await _service.CreateOrderItem(1, 1, 3);

            Assert.Equal(0, product.Quantity);
            Assert.Equal(StatusProduct.OutOfStock, product.Status);
        }

        [Fact]
        public async Task UpdateOrderItem_ValidData_CallsRepository()
        {
            _orderItemRepositoryMock.Setup(r => r.UpdateOrderItem(It.IsAny<OrderItem>()))
                .Returns(Task.CompletedTask);

            var item = new OrderItem { Id = 1, Quantity = 2 };
            await _service.UpdateOrderItem(item);

            _orderItemRepositoryMock.Verify(r => r.UpdateOrderItem(item), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderItem_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateOrderItem(null!));
        }

        [Fact]
        public async Task DeleteOrderItem_RestoresStock()
        {
            var item = new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 3 };
            var product = new Product { Id = 1, Quantity = 2, Status = StatusProduct.InStock };

            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);
            _productRepositoryMock.Setup(r => r.UpdateProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _orderItemRepositoryMock.Setup(r => r.DeleteOrderItem(item))
                .Returns(Task.CompletedTask);

            await _service.DeleteOrderItem(item);

            Assert.Equal(5, product.Quantity);
            _orderItemRepositoryMock.Verify(r => r.DeleteOrderItem(item), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderItem_ProductNotDeleted_HandlesGracefully()
        {
            var item = new OrderItem { Id = 1, OrderId = 1, ProductId = 99, Quantity = 3 };

            _productRepositoryMock.Setup(r => r.GetProductById(99))
                .ReturnsAsync((Product?)null);
            _orderItemRepositoryMock.Setup(r => r.DeleteOrderItem(item))
                .Returns(Task.CompletedTask);

            await _service.DeleteOrderItem(item);

            _orderItemRepositoryMock.Verify(r => r.DeleteOrderItem(item), Times.Once);
        }
    }
}
