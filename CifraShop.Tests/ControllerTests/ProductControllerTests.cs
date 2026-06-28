using CifraShop.API.Controllers;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Products;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CifraShop.Tests.ControllerTests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _controller = new ProductController(_productServiceMock.Object, null!, null!, null!);
        }

        private static Product CreateProduct(int id = 1, string name = "РљСЂСѓР¶РєР°", int price = 100, int quantity = 5, StatusProduct status = StatusProduct.InStock)
            => new() { Id = id, Name = name, Description = "РћРїРёСЃР°РЅРёРµ", Price = price, Quantity = quantity, Status = status };

        [Fact]
        public async Task GetAllProducts_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.GetAllProducts())
                .ReturnsAsync(new List<Product> { CreateProduct(1), CreateProduct(2) });

            var result = await _controller.GetAllProducts();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.Products.ProductResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetAllPaged_ReturnsOk()
        {
            var paged = new Contracts.Responses.Common.PagedResponse<Product>
            {
                Items = new List<Product> { CreateProduct() },
                Page = 0, PageSize = 10, TotalCount = 1
            };
            _productServiceMock.Setup(s => s.GetProductsPaged(0, 10, null, null)).ReturnsAsync(paged);

            var result = await _controller.GetPaged(0, 10);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProductById_Found_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.GetProductById(1)).ReturnsAsync(CreateProduct(1));

            var result = await _controller.GetProductById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Contracts.Responses.Products.ProductResponse>(okResult.Value);
            Assert.Equal(1, response.Id);
        }

        [Fact]
        public async Task GetProductById_NotFound_ReturnsNotFound()
        {
            _productServiceMock.Setup(s => s.GetProductById(99)).ReturnsAsync((Product?)null);

            var result = await _controller.GetProductById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetProductsByName_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.GetProductsByName("РљСЂСѓР¶РєР°"))
                .ReturnsAsync(new List<Product> { CreateProduct() });

            var result = await _controller.GetProductsByName("РљСЂСѓР¶РєР°");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.Products.ProductResponse>>(okResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetProductsByPrice_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.GetProductsByPrice(100))
                .ReturnsAsync(new List<Product> { CreateProduct() });

            var result = await _controller.GetProductsByPrice(100);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProductsByQuantity_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.GetProductsByQuantity(5))
                .ReturnsAsync(new List<Product> { CreateProduct() });

            var result = await _controller.GetProductsByQuantity(5);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProductsByStatus_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.GetProductsByStatus(StatusProduct.InStock))
                .ReturnsAsync(new List<Product> { CreateProduct() });

            var result = await _controller.GetProductsByStatus(StatusProduct.InStock);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task CreateProduct_ValidData_ReturnsOk()
        {
            var product = CreateProduct();
            _productServiceMock.Setup(s => s.CreateProduct("РљСЂСѓР¶РєР°", "РћРїРёСЃР°РЅРёРµ", 100, 5, null))
                .ReturnsAsync(product);

            var result = await _controller.CreateProduct(new CreateProductRequest
            {
                Name = "РљСЂСѓР¶РєР°", Description = "РћРїРёСЃР°РЅРёРµ", Price = 100, Quantity = 5
            });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateProduct_Found_ReturnsNoContent()
        {
            var product = CreateProduct();
            _productServiceMock.Setup(s => s.GetProductById(1)).ReturnsAsync(product);
            _productServiceMock.Setup(s => s.UpdateProduct(It.IsAny<Product>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateProduct(new UpdateProductRequest { Name = "РќРѕРІР°СЏ" }, 1);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("РќРѕРІР°СЏ", product.Name);
        }

        [Fact]
        public async Task UpdateProduct_NotFound_ReturnsNotFound()
        {
            _productServiceMock.Setup(s => s.GetProductById(99)).ReturnsAsync((Product?)null);

            var result = await _controller.UpdateProduct(new UpdateProductRequest { Name = "X" }, 99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesAllFields()
        {
            var product = CreateProduct();
            _productServiceMock.Setup(s => s.GetProductById(1)).ReturnsAsync(product);
            _productServiceMock.Setup(s => s.UpdateProduct(It.IsAny<Product>())).Returns(Task.CompletedTask);

            await _controller.UpdateProduct(new UpdateProductRequest
            {
                Name = "РќРѕРІР°СЏ", Description = "РќРѕРІРѕРµ РѕРїРёСЃР°РЅРёРµ", Price = 200, Quantity = 10, Status = StatusProduct.OutOfStock
            }, 1);

            Assert.Equal("РќРѕРІР°СЏ", product.Name);
            Assert.Equal("РќРѕРІРѕРµ РѕРїРёСЃР°РЅРёРµ", product.Description);
            Assert.Equal(200, product.Price);
            Assert.Equal(10, product.Quantity);
            Assert.Equal(StatusProduct.OutOfStock, product.Status);
        }

        [Fact]
        public async Task DeleteProduct_Found_ReturnsNoContent()
        {
            var product = CreateProduct();
            _productServiceMock.Setup(s => s.GetProductById(1)).ReturnsAsync(product);
            _productServiceMock.Setup(s => s.DeleteProduct(product)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteProduct(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_NotFound_ReturnsNotFound()
        {
            _productServiceMock.Setup(s => s.GetProductById(99)).ReturnsAsync((Product?)null);

            var result = await _controller.DeleteProduct(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task BatchDelete_ValidIds_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.GetProductById(1)).ReturnsAsync(CreateProduct(1));
            _productServiceMock.Setup(s => s.GetProductById(2)).ReturnsAsync(CreateProduct(2));
            _productServiceMock.Setup(s => s.DeleteRange(It.IsAny<List<int>>())).Returns(Task.CompletedTask);

            var result = await _controller.BatchDelete(new BatchDeleteProductsRequest { ProductIds = new List<int> { 1, 2 } });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task BatchDelete_EmptyIds_ReturnsBadRequest()
        {
            var result = await _controller.BatchDelete(new BatchDeleteProductsRequest { ProductIds = new List<int>() });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BatchDelete_NullIds_ReturnsBadRequest()
        {
            var result = await _controller.BatchDelete(new BatchDeleteProductsRequest { ProductIds = null! });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BatchDelete_NoneFound_ReturnsNotFound()
        {
            _productServiceMock.Setup(s => s.GetProductById(It.IsAny<int>())).ReturnsAsync((Product?)null);

            var result = await _controller.BatchDelete(new BatchDeleteProductsRequest { ProductIds = new List<int> { 99 } });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task BatchUpdateStatus_ValidIds_ReturnsOk()
        {
            _productServiceMock.Setup(s => s.UpdateStatusRange(It.IsAny<List<int>>(), StatusProduct.InStock))
                .Returns(Task.CompletedTask);

            var result = await _controller.BatchUpdateStatus(new BatchUpdateProductStatusRequest
            {
                ProductIds = new List<int> { 1, 2 }, NewStatus = StatusProduct.InStock
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task BatchUpdateStatus_EmptyIds_ReturnsBadRequest()
        {
            var result = await _controller.BatchUpdateStatus(new BatchUpdateProductStatusRequest { ProductIds = new List<int>() });

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
