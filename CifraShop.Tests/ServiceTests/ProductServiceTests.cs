using CifraShop.Application.Services.Implementations;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Moq;

namespace CifraShop.Tests.ServiceTests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repositoryMock;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _repositoryMock = new Mock<IProductRepository>();
            _service = new ProductService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsProducts()
        {
            var products = new List<Product> { new() { Id = 1, Name = "Кружка" } };
            _repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(products);

            var result = await _service.GetAllProducts();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetProductById_ReturnsProduct()
        {
            var product = new Product { Id = 1, Name = "Кружка" };
            _repositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync(product);

            var result = await _service.GetProductById(1);

            Assert.NotNull(result);
            Assert.Equal("Кружка", result.Name);
        }

        [Fact]
        public async Task GetProductById_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetProductById(99)).ReturnsAsync((Product?)null);

            var result = await _service.GetProductById(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductsByName_ReturnsProducts()
        {
            var products = new List<Product> { new() { Id = 1, Name = "Кружка" } };
            _repositoryMock.Setup(r => r.GetProductsByName("Кружка")).ReturnsAsync(products);

            var result = await _service.GetProductsByName("Кружка");

            Assert.Single(result);
        }

        [Fact]
        public async Task GetProductsByPrice_ReturnsProducts()
        {
            var products = new List<Product> { new() { Id = 1, Price = 100 } };
            _repositoryMock.Setup(r => r.GetProductsByPrice(100)).ReturnsAsync(products);

            var result = await _service.GetProductsByPrice(100);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetProductsByQuantity_ReturnsProducts()
        {
            var products = new List<Product> { new() { Id = 1, Quantity = 5 } };
            _repositoryMock.Setup(r => r.GetProductsByQuantity(5)).ReturnsAsync(products);

            var result = await _service.GetProductsByQuantity(5);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetProductsByStatus_ReturnsProducts()
        {
            var products = new List<Product> { new() { Id = 1, Status = StatusProduct.InStock } };
            _repositoryMock.Setup(r => r.GetProductsByStatus(StatusProduct.InStock)).ReturnsAsync(products);

            var result = await _service.GetProductsByStatus(StatusProduct.InStock);

            Assert.Single(result);
            Assert.Equal(StatusProduct.InStock, result[0].Status);
        }

        [Fact]
        public async Task GetProductsPaged_ReturnsPagedResponse()
        {
            var products = new List<Product> { new() { Id = 1 } };
            _repositoryMock.Setup(r => r.GetAllPaged(0, 10, null, null))
                .ReturnsAsync((products, 1));

            var result = await _service.GetProductsPaged(0, 10);

            Assert.IsType<PagedResponse<Product>>(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetProductsPaged_NegativePage_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProductsPaged(-1, 10));
        }

        [Fact]
        public async Task GetProductsPaged_ZeroPageSize_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProductsPaged(0, 0));
        }

        [Fact]
        public async Task CreateProduct_ValidData_CreatesProduct()
        {
            _repositoryMock.Setup(r => r.AddProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateProduct("Кружка", "Описание", 100, 5);

            Assert.Equal("Кружка", result.Name);
            Assert.Equal("Описание", result.Description);
            Assert.Equal(100, result.Price);
            Assert.Equal(5, result.Quantity);
            Assert.Equal(StatusProduct.InStock, result.Status);
            _repositoryMock.Verify(r => r.AddProduct(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WithImageUrl_SetsImageUrl()
        {
            _repositoryMock.Setup(r => r.AddProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateProduct("Кружка", "Описание", 100, 5, "http://img.png");

            Assert.Equal("http://img.png", result.ImageUrl);
        }

        [Fact]
        public async Task CreateProduct_ZeroQuantity_SetsOutOfStock()
        {
            _repositoryMock.Setup(r => r.AddProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var result = await _service.CreateProduct("Кружка", "Описание", 100, 0);

            Assert.Equal(StatusProduct.OutOfStock, result.Status);
        }

        [Fact]
        public async Task CreateProduct_EmptyName_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduct("", "Описание", 100, 5));
        }

        [Fact]
        public async Task CreateProduct_NegativePrice_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduct("Кружка", "Описание", -1, 5));
        }

        [Fact]
        public async Task CreateProduct_NegativeQuantity_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProduct("Кружка", "Описание", 100, -1));
        }

        [Fact]
        public async Task UpdateProduct_ValidData_CallsRepository()
        {
            _repositoryMock.Setup(r => r.UpdateProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var product = new Product { Id = 1, Name = "Кружка", Quantity = 5 };
            await _service.UpdateProduct(product);

            _repositoryMock.Verify(r => r.UpdateProduct(product), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateProduct(null!));
        }

        [Fact]
        public async Task UpdateProduct_EmptyName_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateProduct(new Product { Id = 1, Name = "", Quantity = 5 }));
        }

        [Fact]
        public async Task UpdateProduct_NegativeQuantity_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateProduct(new Product { Id = 1, Name = "Кружка", Quantity = -1 }));
        }

        [Fact]
        public async Task DeleteProduct_DelegatesToRepository()
        {
            var product = new Product { Id = 1 };
            _repositoryMock.Setup(r => r.DeleteProduct(product)).Returns(Task.CompletedTask);

            await _service.DeleteProduct(product);

            _repositoryMock.Verify(r => r.DeleteProduct(product), Times.Once);
        }

        [Fact]
        public async Task DeleteRange_ValidIds_CallsRepository()
        {
            _repositoryMock.Setup(r => r.DeleteRange(It.IsAny<List<int>>()))
                .Returns(Task.CompletedTask);

            await _service.DeleteRange(new List<int> { 1, 2 });

            _repositoryMock.Verify(r => r.DeleteRange(It.Is<List<int>>(l => l.Count == 2)), Times.Once);
        }

        [Fact]
        public async Task DeleteRange_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteRange(null!));
        }

        [Fact]
        public async Task DeleteRange_EmptyList_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteRange(new List<int>()));
        }

        [Fact]
        public async Task UpdateStatusRange_ValidIds_CallsRepository()
        {
            _repositoryMock.Setup(r => r.UpdateStatusRange(It.IsAny<List<int>>(), StatusProduct.InStock))
                .Returns(Task.CompletedTask);

            await _service.UpdateStatusRange(new List<int> { 1, 2 }, StatusProduct.InStock);

            _repositoryMock.Verify(r => r.UpdateStatusRange(It.Is<List<int>>(l => l.Count == 2), StatusProduct.InStock), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusRange_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateStatusRange(null!, StatusProduct.InStock));
        }

        [Fact]
        public async Task UpdateStatusRange_EmptyList_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateStatusRange(new List<int>(), StatusProduct.InStock));
        }
    }
}
