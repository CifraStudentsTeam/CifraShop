using CifraShop.Application.Services.Implementations;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Moq;

namespace CifraShop.Tests.ServiceTests
{
    public class NotificationSettingsServiceTests
    {
        private readonly Mock<INotificationSettingsRepository> _repositoryMock;
        private readonly NotificationSettingsService _service;

        public NotificationSettingsServiceTests()
        {
            _repositoryMock = new Mock<INotificationSettingsRepository>();
            _service = new NotificationSettingsService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsSettings()
        {
            var settings = new List<NotificationSettings>
            {
                new() { Id = 1, Branch = "Филиал 1" },
                new() { Id = 2, Branch = "Филиал 2" }
            };
            _repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(settings);

            var result = await _service.GetAll();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetById_ReturnsSettings()
        {
            var settings = new NotificationSettings { Id = 1, Branch = "Филиал 1" };
            _repositoryMock.Setup(r => r.GetById(1)).ReturnsAsync(settings);

            var result = await _service.GetById(1);

            Assert.NotNull(result);
            Assert.Equal("Филиал 1", result.Branch);
        }

        [Fact]
        public async Task GetById_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetById(99))
                .ReturnsAsync((NotificationSettings?)null);

            var result = await _service.GetById(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByBranch_ReturnsSettings()
        {
            var settings = new NotificationSettings { Id = 1, Branch = "Филиал 1" };
            _repositoryMock.Setup(r => r.GetByBranch("Филиал 1")).ReturnsAsync(settings);

            var result = await _service.GetByBranch("Филиал 1");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Create_ValidData_CreatesSettings()
        {
            _repositoryMock.Setup(r => r.GetByBranch("Филиал 1"))
                .ReturnsAsync((NotificationSettings?)null);
            _repositoryMock.Setup(r => r.Add(It.IsAny<NotificationSettings>()))
                .Returns(Task.CompletedTask);

            var result = await _service.Create(
                "email@test.com", "token", "chatId", "Филиал 1",
                true, true, true, 5);

            Assert.Equal("Филиал 1", result.Branch);
            Assert.Equal("email@test.com", result.Email);
            Assert.True(result.NotifyOnNewOrder);
            Assert.Equal(5, result.LowStockThreshold);
            _repositoryMock.Verify(r => r.Add(It.IsAny<NotificationSettings>()), Times.Once);
        }

        [Fact]
        public async Task Create_EmptyBranch_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Create("email@test.com", "token", "chatId", "", true, true, true, 5));
        }

        [Fact]
        public async Task Create_NegativeThreshold_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Create("email@test.com", "token", "chatId", "Филиал 1", true, true, true, -1));
        }

        [Fact]
        public async Task Create_DuplicateBranch_Throws()
        {
            var existing = new NotificationSettings { Id = 1, Branch = "Филиал 1" };
            _repositoryMock.Setup(r => r.GetByBranch("Филиал 1")).ReturnsAsync(existing);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Create("email@test.com", "token", "chatId", "Филиал 1", true, true, true, 5));
        }

        [Fact]
        public async Task Create_NullEmail_DefaultsEmpty()
        {
            _repositoryMock.Setup(r => r.GetByBranch("Филиал 1"))
                .ReturnsAsync((NotificationSettings?)null);
            _repositoryMock.Setup(r => r.Add(It.IsAny<NotificationSettings>()))
                .Returns(Task.CompletedTask);

            var result = await _service.Create(
                null, null, null, "Филиал 1",
                false, false, false, 0);

            Assert.Equal(string.Empty, result.Email);
            Assert.Equal(string.Empty, result.TelegramBotToken);
            Assert.Equal(string.Empty, result.TelegramChatId);
        }

        [Fact]
        public async Task Update_UpdatesSettings()
        {
            var settings = new NotificationSettings { Id = 1, Branch = "Филиал 1", LowStockThreshold = 10 };
            _repositoryMock.Setup(r => r.Update(settings)).Returns(Task.CompletedTask);

            await _service.Update(settings);

            _repositoryMock.Verify(r => r.Update(settings), Times.Once);
        }

        [Fact]
        public async Task Delete_DeletesSettings()
        {
            var settings = new NotificationSettings { Id = 1 };
            _repositoryMock.Setup(r => r.Delete(settings)).Returns(Task.CompletedTask);

            await _service.Delete(settings);

            _repositoryMock.Verify(r => r.Delete(settings), Times.Once);
        }
    }
}
