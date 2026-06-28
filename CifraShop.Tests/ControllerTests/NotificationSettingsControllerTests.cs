using CifraShop.API.Controllers;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Notifications;
using CifraShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CifraShop.Tests.ControllerTests
{
    public class NotificationSettingsControllerTests
    {
        private readonly Mock<INotificationSettingsService> _serviceMock;
        private readonly NotificationSettingsController _controller;

        // Инициализация мок INotificationSettingsService и создание экземпляра NotificationSettingsController перед каждым тестом.
        public NotificationSettingsControllerTests()
        {
            _serviceMock = new Mock<INotificationSettingsService>();
            _controller = new NotificationSettingsController(_serviceMock.Object);
        }

        private static NotificationSettings CreateSettings(int id = 1, string branch = "Филиал 1")
            => new() { Id = id, Branch = branch, Email = "a@b.com", TelegramBotToken = "token", TelegramChatId = "chat" };

        [Fact]
        // Проверка, что GetAll возвращает статус 200 OK со списком из двух настроек уведомлений
        public async Task CheckingGetAllReturnsOKWith2Settings()
        {
            _serviceMock.Setup(s => s.GetAll())
                .ReturnsAsync(new List<NotificationSettings> { CreateSettings(1), CreateSettings(2, "Филиал 2") });

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.Notifications.NotificationSettingsResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        // Проверка, что при нахождении настроек по ID возвращается стаус 200 OK с корректным идентификатором.
        public async Task CheckingFoundByIdReturnsOKWithCorrectId()
        {
            _serviceMock.Setup(s => s.GetById(1)).ReturnsAsync(CreateSettings(1));

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Contracts.Responses.Notifications.NotificationSettingsResponse>(okResult.Value);
            Assert.Equal(1, response.Id);
        }

        [Fact]
        // Проверка, что при отсутствии настроек с ID 99 возвращается статус 404 Not Found
        public async Task CheckingMissingIdReturns404()
        {
            _serviceMock.Setup(s => s.GetById(99)).ReturnsAsync((NotificationSettings?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Проверка, что при наличии настроек для филиала "Филиал 1" возвращается статус 200 OK с непустым телом.
        public async Task CheckingFoundBranchReturns200OK()
        {
            _serviceMock.Setup(s => s.GetByBranch("Филиал 1")).ReturnsAsync(CreateSettings(1, "Филиал 1"));

            var result = await _controller.GetByBranch("Филиал 1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // Проверка, что при отсутствии настроек для несуществующего филиала "X" возвращается статус 404 Not Found.
        public async Task CheckingMissingBranchReturns404()
        {
            _serviceMock.Setup(s => s.GetByBranch("X")).ReturnsAsync((NotificationSettings?)null);

            var result = await _controller.GetByBranch("X");

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Проверка, что при валидных данных создание настроек уведомлений возвращает статус 200 OK с непустым телом.
        public async Task CheckingValidCreationReturns200Ok()
        {
            var settings = CreateSettings();
            _serviceMock.Setup(s => s.Create(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(settings);
            _serviceMock.Setup(s => s.Update(It.IsAny<NotificationSettings>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(new CreateNotificationSettingsRequest
            {
                Email = "a@b.com", Branch = "Филиал 1",
                TelegramBotToken = "token", TelegramChatId = "chat"
            });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // Проверка, что при успешном обновлении существующих настроек возвращается статус 204 No Content, а email в настройках обновляется.
        public async Task CheckingUpdateReturns204AndUpdatesEmail()
        {
            var settings = CreateSettings();
            _serviceMock.Setup(s => s.GetById(1)).ReturnsAsync(settings);
            _serviceMock.Setup(s => s.Update(It.IsAny<NotificationSettings>())).Returns(Task.CompletedTask);

            var result = await _controller.Update(1, new CreateNotificationSettingsRequest
            {
                Email = "new@b.com", Branch = "Филиал 1",
                TelegramBotToken = "token", TelegramChatId = "chat"
            });

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("new@b.com", settings.Email);
        }

        [Fact]
        //Проверка, что при попытке обновления несуществующих настроек(ID 99) возвращается статус  404 Not Found
        public async Task CheckingUpdateMissingSettingsReturns404()
        {
            _serviceMock.Setup(s => s.GetById(99)).ReturnsAsync((NotificationSettings?)null);

            var result = await _controller.Update(99, new CreateNotificationSettingsRequest
            {
                Branch = "X"
            });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        // Проверка, что удаление существующих настроек уведомлений (ID 1) возвращает статус 204 No Content.
        public async Task CheckingFoundDeleteReturns204()
        {
            var settings = CreateSettings();
            _serviceMock.Setup(s => s.GetById(1)).ReturnsAsync(settings);
            _serviceMock.Setup(s => s.Delete(settings)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        // Проверка, что удаление несуществующих настроек (ID 99) возвращает статус 404 Not Found.
        public async Task CheckingDeleteMissingReturns404()
        {
            _serviceMock.Setup(s => s.GetById(99)).ReturnsAsync((NotificationSettings?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
