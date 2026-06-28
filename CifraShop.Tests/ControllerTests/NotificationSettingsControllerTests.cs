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

        // Р ВР Р…Р С‘РЎвЂ Р С‘Р В°Р В»Р С‘Р В·Р В°РЎвЂ Р С‘РЎРЏ Р СР С•Р С” INotificationSettingsService Р С‘ РЎРѓР С•Р В·Р Т‘Р В°Р Р…Р С‘Р Вµ РЎРЊР С”Р В·Р ВµР СР С—Р В»РЎРЏРЎР‚Р В° NotificationSettingsController Р С—Р ВµРЎР‚Р ВµР Т‘ Р С”Р В°Р В¶Р Т‘РЎвЂ№Р С РЎвЂљР ВµРЎРѓРЎвЂљР С•Р С.
        public NotificationSettingsControllerTests()
        {
            _serviceMock = new Mock<INotificationSettingsService>();
            _controller = new NotificationSettingsController(_serviceMock.Object, null!);
        }

        private static NotificationSettings CreateSettings(int id = 1, string branch = "Р В¤Р С‘Р В»Р С‘Р В°Р В» 1")
            => new() { Id = id, Branch = branch, Email = "a@b.com", };

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• GetAll Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 200 OK РЎРѓР С• РЎРѓР С—Р С‘РЎРѓР С”Р С•Р С Р С‘Р В· Р Т‘Р Р†РЎС“РЎвЂ¦ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” РЎС“Р Р†Р ВµР Т‘Р С•Р СР В»Р ВµР Р…Р С‘Р в„–
        public async Task CheckingGetAllReturnsOKWith2Settings()
        {
            _serviceMock.Setup(s => s.GetAll())
                .ReturnsAsync(new List<NotificationSettings> { CreateSettings(1), CreateSettings(2, "Р В¤Р С‘Р В»Р С‘Р В°Р В» 2") });

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.Notifications.NotificationSettingsResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• Р С—РЎР‚Р С‘ Р Р…Р В°РЎвЂ¦Р С•Р В¶Р Т‘Р ВµР Р…Р С‘Р С‘ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” Р С—Р С• ID Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљРЎРѓРЎРЏ РЎРѓРЎвЂљР В°РЎС“РЎРѓ 200 OK РЎРѓ Р С”Р С•РЎР‚РЎР‚Р ВµР С”РЎвЂљР Р…РЎвЂ№Р С Р С‘Р Т‘Р ВµР Р…РЎвЂљР С‘РЎвЂћР С‘Р С”Р В°РЎвЂљР С•РЎР‚Р С•Р С.
        public async Task CheckingFoundByIdReturnsOKWithCorrectId()
        {
            _serviceMock.Setup(s => s.GetById(1)).ReturnsAsync(CreateSettings(1));

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Contracts.Responses.Notifications.NotificationSettingsResponse>(okResult.Value);
            Assert.Equal(1, response.Id);
        }

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• Р С—РЎР‚Р С‘ Р С•РЎвЂљРЎРѓРЎС“РЎвЂљРЎРѓРЎвЂљР Р†Р С‘Р С‘ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” РЎРѓ ID 99 Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљРЎРѓРЎРЏ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 404 Not Found
        public async Task CheckingMissingIdReturns404()
        {
            _serviceMock.Setup(s => s.GetById(99)).ReturnsAsync((NotificationSettings?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• Р С—РЎР‚Р С‘ Р Р…Р В°Р В»Р С‘РЎвЂЎР С‘Р С‘ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” Р Т‘Р В»РЎРЏ РЎвЂћР С‘Р В»Р С‘Р В°Р В»Р В° "Р В¤Р С‘Р В»Р С‘Р В°Р В» 1" Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљРЎРѓРЎРЏ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 200 OK РЎРѓ Р Р…Р ВµР С—РЎС“РЎРѓРЎвЂљРЎвЂ№Р С РЎвЂљР ВµР В»Р С•Р С.
        public async Task CheckingFoundBranchReturns200OK()
        {
            _serviceMock.Setup(s => s.GetByBranch("Р В¤Р С‘Р В»Р С‘Р В°Р В» 1")).ReturnsAsync(CreateSettings(1, "Р В¤Р С‘Р В»Р С‘Р В°Р В» 1"));

            var result = await _controller.GetByBranch("Р В¤Р С‘Р В»Р С‘Р В°Р В» 1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• Р С—РЎР‚Р С‘ Р С•РЎвЂљРЎРѓРЎС“РЎвЂљРЎРѓРЎвЂљР Р†Р С‘Р С‘ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” Р Т‘Р В»РЎРЏ Р Р…Р ВµРЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“РЎР‹РЎвЂ°Р ВµР С–Р С• РЎвЂћР С‘Р В»Р С‘Р В°Р В»Р В° "X" Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљРЎРѓРЎРЏ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 404 Not Found.
        public async Task CheckingMissingBranchReturns404()
        {
            _serviceMock.Setup(s => s.GetByBranch("X")).ReturnsAsync((NotificationSettings?)null);

            var result = await _controller.GetByBranch("X");

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• Р С—РЎР‚Р С‘ Р Р†Р В°Р В»Р С‘Р Т‘Р Р…РЎвЂ№РЎвЂ¦ Р Т‘Р В°Р Р…Р Р…РЎвЂ№РЎвЂ¦ РЎРѓР С•Р В·Р Т‘Р В°Р Р…Р С‘Р Вµ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” РЎС“Р Р†Р ВµР Т‘Р С•Р СР В»Р ВµР Р…Р С‘Р в„– Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 200 OK РЎРѓ Р Р…Р ВµР С—РЎС“РЎРѓРЎвЂљРЎвЂ№Р С РЎвЂљР ВµР В»Р С•Р С.
        public async Task CheckingValidCreationReturns200Ok()
        {
            var settings = CreateSettings();
            _serviceMock.Setup(s => s.Create(
                It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(settings);
            _serviceMock.Setup(s => s.Update(It.IsAny<NotificationSettings>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(new CreateNotificationSettingsRequest
            {
                Email = "a@b.com", Branch = "Р В¤Р С‘Р В»Р С‘Р В°Р В» 1",
                });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• Р С—РЎР‚Р С‘ РЎС“РЎРѓР С—Р ВµРЎв‚¬Р Р…Р С•Р С Р С•Р В±Р Р…Р С•Р Р†Р В»Р ВµР Р…Р С‘Р С‘ РЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“РЎР‹РЎвЂ°Р С‘РЎвЂ¦ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљРЎРѓРЎРЏ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 204 No Content, Р В° email Р Р† Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р в„–Р С”Р В°РЎвЂ¦ Р С•Р В±Р Р…Р С•Р Р†Р В»РЎРЏР ВµРЎвЂљРЎРѓРЎРЏ.
        public async Task CheckingUpdateReturns204AndUpdatesEmail()
        {
            var settings = CreateSettings();
            _serviceMock.Setup(s => s.GetById(1)).ReturnsAsync(settings);
            _serviceMock.Setup(s => s.Update(It.IsAny<NotificationSettings>())).Returns(Task.CompletedTask);

            var result = await _controller.Update(1, new CreateNotificationSettingsRequest
            {
                Email = "new@b.com", Branch = "Р В¤Р С‘Р В»Р С‘Р В°Р В» 1",
                });

            Assert.IsType<NoContentResult>(result);
            Assert.Equal("new@b.com", settings.Email);
        }

        [Fact]
        //Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• Р С—РЎР‚Р С‘ Р С—Р С•Р С—РЎвЂ№РЎвЂљР С”Р Вµ Р С•Р В±Р Р…Р С•Р Р†Р В»Р ВµР Р…Р С‘РЎРЏ Р Р…Р ВµРЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“РЎР‹РЎвЂ°Р С‘РЎвЂ¦ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С”(ID 99) Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљРЎРѓРЎРЏ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ  404 Not Found
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
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• РЎС“Р Т‘Р В°Р В»Р ВµР Р…Р С‘Р Вµ РЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“РЎР‹РЎвЂ°Р С‘РЎвЂ¦ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” РЎС“Р Р†Р ВµР Т‘Р С•Р СР В»Р ВµР Р…Р С‘Р в„– (ID 1) Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 204 No Content.
        public async Task CheckingFoundDeleteReturns204()
        {
            var settings = CreateSettings();
            _serviceMock.Setup(s => s.GetById(1)).ReturnsAsync(settings);
            _serviceMock.Setup(s => s.Delete(settings)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        // Р СџРЎР‚Р С•Р Р†Р ВµРЎР‚Р С”Р В°, РЎвЂЎРЎвЂљР С• РЎС“Р Т‘Р В°Р В»Р ВµР Р…Р С‘Р Вµ Р Р…Р ВµРЎРѓРЎС“РЎвЂ°Р ВµРЎРѓРЎвЂљР Р†РЎС“РЎР‹РЎвЂ°Р С‘РЎвЂ¦ Р Р…Р В°РЎРѓРЎвЂљРЎР‚Р С•Р ВµР С” (ID 99) Р Р†Р С•Р В·Р Р†РЎР‚Р В°РЎвЂ°Р В°Р ВµРЎвЂљ РЎРѓРЎвЂљР В°РЎвЂљРЎС“РЎРѓ 404 Not Found.
        public async Task CheckingDeleteMissingReturns404()
        {
            _serviceMock.Setup(s => s.GetById(99)).ReturnsAsync((NotificationSettings?)null);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
