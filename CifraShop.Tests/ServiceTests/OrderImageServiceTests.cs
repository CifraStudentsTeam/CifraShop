using CifraShop.Application.Services.Implementations;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Moq;

namespace CifraShop.Tests.ServiceTests
{
    public class OrderImageServiceTests
    {
        private readonly Mock<IOrderImageRepository> _repositoryMock;
        private readonly OrderImageService _service;

        public OrderImageServiceTests()
        {
            _repositoryMock = new Mock<IOrderImageRepository>();
            _service = new OrderImageService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetByOrderId_ReturnsImages()
        {
            var images = new List<OrderImage> { new() { Id = 1, OrderId = 1, FileName = "img.png" } };
            _repositoryMock.Setup(r => r.GetByOrderId(1)).ReturnsAsync(images);

            var result = await _service.GetByOrderId(1);

            Assert.Single(result);
            Assert.Equal("img.png", result[0].FileName);
        }

        [Fact]
        public async Task GetById_ReturnsImage()
        {
            var image = new OrderImage { Id = 1, FileName = "receipt.png" };
            _repositoryMock.Setup(r => r.GetById(1)).ReturnsAsync(image);

            var result = await _service.GetById(1);

            Assert.NotNull(result);
            Assert.Equal("receipt.png", result.FileName);
        }

        [Fact]
        public async Task GetById_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetById(99)).ReturnsAsync((OrderImage?)null);

            var result = await _service.GetById(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCountByOrderId_ReturnsCount()
        {
            _repositoryMock.Setup(r => r.GetCountByOrderId(1)).ReturnsAsync(3);

            var result = await _service.GetCountByOrderId(1);

            Assert.Equal(3, result);
        }

        [Fact]
        public async Task Add_AddsImage()
        {
            var image = new OrderImage { Id = 1, FileName = "new.png" };
            _repositoryMock.Setup(r => r.Add(image)).Returns(Task.CompletedTask);

            var result = await _service.Add(image);

            Assert.NotNull(result);
            Assert.Equal("new.png", result.FileName);
            _repositoryMock.Verify(r => r.Add(image), Times.Once);
        }

        [Fact]
        public async Task Update_UpdatesImage()
        {
            var image = new OrderImage { Id = 1 };
            _repositoryMock.Setup(r => r.Update(image)).Returns(Task.CompletedTask);

            await _service.Update(image);

            _repositoryMock.Verify(r => r.Update(image), Times.Once);
        }

        [Fact]
        public async Task Delete_DeletesImage()
        {
            var image = new OrderImage { Id = 1 };
            _repositoryMock.Setup(r => r.Delete(image)).Returns(Task.CompletedTask);

            await _service.Delete(image);

            _repositoryMock.Verify(r => r.Delete(image), Times.Once);
        }
    }
}
