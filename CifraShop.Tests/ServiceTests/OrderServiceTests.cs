using CifraShop.Application.Services.Implementations;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Moq;

namespace CifraShop.Tests.ServiceTests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _service = new OrderService(
                _orderRepositoryMock.Object,
                _orderItemRepositoryMock.Object,
                _userRepositoryMock.Object,
                _productRepositoryMock.Object,
                null!);
        }

        [Fact]
        public async Task GetAllOrders_ReturnsOrders()
        {
            var orders = new List<Order> { new() { Id = 1 } };
            _orderRepositoryMock.Setup(r => r.GetAll()).ReturnsAsync(orders);

            var result = await _service.GetAllOrders();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetOrderById_ReturnsOrder()
        {
            var order = new Order { Id = 1 };
            _orderRepositoryMock.Setup(r => r.GetOrderById(1)).ReturnsAsync(order);

            var result = await _service.GetOrderById(1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetOrderById_ReturnsNull()
        {
            _orderRepositoryMock.Setup(r => r.GetOrderById(99)).ReturnsAsync((Order?)null);

            var result = await _service.GetOrderById(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrdersByStatus_ReturnsOrders()
        {
            var orders = new List<Order> { new() { Id = 1, Status = StatusOrder.Completed } };
            _orderRepositoryMock.Setup(r => r.GetOrdersByStatus(StatusOrder.Completed)).ReturnsAsync(orders);

            var result = await _service.GetOrdersByStatus(StatusOrder.Completed);

            Assert.Single(result);
            Assert.Equal(StatusOrder.Completed, result[0].Status);
        }

        [Fact]
        public async Task GetOrdersBySum_ReturnsOrders()
        {
            var orders = new List<Order> { new() { Id = 1, Sum = 100 } };
            _orderRepositoryMock.Setup(r => r.GetOrdersBySum(100)).ReturnsAsync(orders);

            var result = await _service.GetOrdersBySum(100);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetOrdersByCustomerEmail_ReturnsOrders()
        {
            var orders = new List<Order> { new() { Id = 1 } };
            _orderRepositoryMock.Setup(r => r.GetOrdersByCustomerEmail("test@email.com")).ReturnsAsync(orders);

            var result = await _service.GetOrdersByCustomerEmail("test@email.com");

            Assert.Single(result);
        }

        [Fact]
        public async Task GetOrdersByDateRange_ReturnsOrders()
        {
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 12, 31);
            var orders = new List<Order> { new() { Id = 1 } };
            _orderRepositoryMock.Setup(r => r.GetOrdersByDateRange(from, to)).ReturnsAsync(orders);

            var result = await _service.GetOrdersByDateRange(from, to);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetOrdersPaged_ReturnsPagedResponse()
        {
            var orders = new List<Order> { new() { Id = 1 } };
            _orderRepositoryMock.Setup(r => r.GetAllPaged(0, 10, null, null, null, null))
                .ReturnsAsync((orders, 1));

            var result = await _service.GetOrdersPaged(0, 10);

            Assert.IsType<PagedResponse<Order>>(result);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task GetOrdersPaged_NegativePage_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetOrdersPaged(-1, 10));
        }

        [Fact]
        public async Task GetOrdersPaged_ZeroPageSize_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetOrdersPaged(0, 0));
        }

        [Fact]
        public async Task CreateOrder_ValidData_CreatesOrder()
        {
            var user = new User { Id = 1, Email = "test@email.com" };
            var product = new Product { Id = 1, Name = "Р В РЎв„ўР РЋР вЂљР РЋРЎвЂњР В Р’В¶Р В РЎвЂќР В Р’В°", Price = 100, Quantity = 5 };
            var order = new Order { Id = 1, Sum = 200, Status = StatusOrder.Pending, CustomerId = 1 };

            _userRepositoryMock.Setup(r => r.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);
            _orderRepositoryMock.Setup(r => r.CreateOrderInTransaction(
                It.IsAny<Order>(), It.IsAny<List<OrderItem>>(), It.IsAny<List<(int, int)>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(order);

            var result = await _service.CreateOrder("test@email.com", new List<(int, int)> { (1, 2) }, "");

            Assert.NotNull(result);
            Assert.Equal(200, result.Sum);
        }

        [Fact]
        public async Task CreateOrder_EmptyEmail_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrder("", new List<(int, int)> { (1, 1) }));
        }

        [Fact]
        public async Task CreateOrder_EmptyItems_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrder("test@email.com", new List<(int, int)>()));
        }

        [Fact]
        public async Task CreateOrder_NullItems_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrder("test@email.com", null!));
        }

        [Fact]
        public async Task CreateOrder_DuplicateProducts_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrder("test@email.com", new List<(int, int)> { (1, 1), (1, 2) }));
        }

        [Fact]
        public async Task CreateOrder_UserNotFound_Throws()
        {
            _userRepositoryMock.Setup(r => r.GetUserByEmail("no@email.com"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateOrder("no@email.com", new List<(int, int)> { (1, 1) }));
        }

        [Fact]
        public async Task CreateOrder_ProductNotFound_Throws()
        {
            var user = new User { Id = 1, Email = "test@email.com" };
            _userRepositoryMock.Setup(r => r.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _productRepositoryMock.Setup(r => r.GetProductById(1))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateOrder("test@email.com", new List<(int, int)> { (1, 1) }));
        }

        [Fact]
        public async Task CreateOrder_InsufficientStock_Throws()
        {
            var user = new User { Id = 1, Email = "test@email.com" };
            var product = new Product { Id = 1, Name = "Р В РЎв„ўР РЋР вЂљР РЋРЎвЂњР В Р’В¶Р В РЎвЂќР В Р’В°", Price = 100, Quantity = 2 };
            _userRepositoryMock.Setup(r => r.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateOrder("test@email.com", new List<(int, int)> { (1, 5) }));
        }

        [Fact]
        public async Task CreateOrder_ZeroQuantity_Throws()
        {
            var user = new User { Id = 1, Email = "test@email.com" };
            var product = new Product { Id = 1, Name = "Р В РЎв„ўР РЋР вЂљР РЋРЎвЂњР В Р’В¶Р В РЎвЂќР В Р’В°", Price = 100, Quantity = 5 };
            _userRepositoryMock.Setup(r => r.GetUserByEmail("test@email.com")).ReturnsAsync(user);
            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrder("test@email.com", new List<(int, int)> { (1, 0) }));
        }

        [Fact]
        public async Task UpdateOrder_ValidData_CallsRepository()
        {
            _orderRepositoryMock.Setup(r => r.UpdateOrder(It.IsAny<Order>()))
                .Returns(Task.CompletedTask);

            var order = new Order { Id = 1, Status = StatusOrder.Paid };
            await _service.UpdateOrder(order);

            _orderRepositoryMock.Verify(r => r.UpdateOrder(order), Times.Once);
        }

        [Fact]
        public async Task UpdateOrder_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateOrder(null!));
        }

        [Fact]
        public async Task UpdateStatusRange_ValidIds_CallsRepository()
        {
            _orderRepositoryMock.Setup(r => r.UpdateStatusRange(It.IsAny<List<int>>(), StatusOrder.Paid))
                .Returns(Task.CompletedTask);

            await _service.UpdateStatusRange(new List<int> { 1, 2 }, StatusOrder.Paid);

            _orderRepositoryMock.Verify(r => r.UpdateStatusRange(It.Is<List<int>>(l => l.Count == 2), StatusOrder.Paid), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusRange_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateStatusRange(null!, StatusOrder.Paid));
        }

        [Fact]
        public async Task UpdateStatusRange_EmptyList_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateStatusRange(new List<int>(), StatusOrder.Paid));
        }

        [Fact]
        public async Task DeleteOrder_RestoresStockAndDeletesItems()
        {
            var order = new Order { Id = 1 };
            var product = new Product { Id = 1, Quantity = 2, Status = StatusProduct.InStock };
            var orderItems = new List<OrderItem>
            {
                new() { Id = 1, OrderId = 1, ProductId = 1, Quantity = 3, Price = 100 }
            };

            _orderItemRepositoryMock.Setup(r => r.GetOrderItemsByOrderId(1)).ReturnsAsync(orderItems);
            _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);
            _productRepositoryMock.Setup(r => r.UpdateProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            _orderItemRepositoryMock.Setup(r => r.DeleteOrderItem(It.IsAny<OrderItem>()))
                .Returns(Task.CompletedTask);
            _orderRepositoryMock.Setup(r => r.DeleteOrder(order)).Returns(Task.CompletedTask);

            await _service.DeleteOrder(order);

            Assert.Equal(5, product.Quantity);
            _orderItemRepositoryMock.Verify(r => r.DeleteOrderItem(It.IsAny<OrderItem>()), Times.Once);
            _orderRepositoryMock.Verify(r => r.DeleteOrder(order), Times.Once);
        }

        [Fact]
        public async Task DeleteOrder_ProductDeleted_HandlesGracefully()
        {
            var order = new Order { Id = 1 };
            var orderItems = new List<OrderItem>
            {
                new() { Id = 1, OrderId = 1, ProductId = 99, Quantity = 3, Price = 100 }
            };

            _orderItemRepositoryMock.Setup(r => r.GetOrderItemsByOrderId(1)).ReturnsAsync(orderItems);
            _productRepositoryMock.Setup(r => r.GetProductById(99)).ReturnsAsync((Product?)null);
            _orderItemRepositoryMock.Setup(r => r.DeleteOrderItem(It.IsAny<OrderItem>()))
                .Returns(Task.CompletedTask);
            _orderRepositoryMock.Setup(r => r.DeleteOrder(order)).Returns(Task.CompletedTask);

            await _service.DeleteOrder(order);

            _orderRepositoryMock.Verify(r => r.DeleteOrder(order), Times.Once);
        }
    }
}
