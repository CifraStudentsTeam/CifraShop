using CifraShop.API.Controllers;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.AdminAction;
using CifraShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CifraShop.Tests.ControllerTests
{
    public class AdminActionControllerTests
    {
        private readonly Mock<IAdminActionService> _serviceMock;
        private readonly AdminActionController _controller;

        // Инициализирует мок IAdminActionService и создаёт экземпляр AdminActionController перед каждым тестом.
        public AdminActionControllerTests()
        {
            _serviceMock = new Mock<IAdminActionService>();
            _controller = new AdminActionController(_serviceMock.Object);
        }

        [Fact]
        // Проверка, что метод GetLast возвращает стаус 200 OK и список из двух действий администратора.
        public async Task CheckingGetLastReturnsOKWith2Actions()
        {
            var actions = new List<AdminAction>
            {
                new() { Id = 1, ActionType = "Create", Details = "Создан товар", Branch = "Филиал 1", CreatedAt = DateTime.UtcNow },
                new() { Id = 2, ActionType = "Update", Details = "Обновлён товар", Branch = "Филиал 1", CreatedAt = DateTime.UtcNow }
            };
            _serviceMock.Setup(s => s.GetLastActions(50, null)).ReturnsAsync(actions);

            var result = await _controller.GetLast();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.AdminAction.AdminActionResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        // Проверка, что GetLast с параметром филиала возвращает только действия этого филиала.
        public async Task CheckingFilteredByBranchReturnsSingleAction()
        {
            var actions = new List<AdminAction>
            {
                new() { Id = 1, ActionType = "Create", Details = "Details", Branch = "Филиал 1", CreatedAt = DateTime.UtcNow }
            };
            _serviceMock.Setup(s => s.GetLastActions(10, "Филиал 1")).ReturnsAsync(actions);

            var result = await _controller.GetLast(10, "Филиал 1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.AdminAction.AdminActionResponse>>(okResult.Value);
            Assert.Single(list);
            Assert.Equal("Филиал 1", list[0].Branch);
        }

        [Fact]
        // Проверка, что при отсутствии действий возвращается статус 200 OK с пустым списком.
        public async Task CheckingEmptyListReturnsOKWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetLastActions(50, null)).ReturnsAsync(new List<AdminAction>());

            var result = await _controller.GetLast();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.AdminAction.AdminActionResponse>>(okResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        // Проверка, что при валидных данных создание действия возвращает статус 200 OK и вызывает AddAction с корректными параметрами.
        public async Task CheckingValidCreationReturnsOKAndCallsAddActionOnce()
        {
            _serviceMock.Setup(s => s.AddAction("Create", "Создан товар", "Филиал 1"))
                .Returns(Task.CompletedTask);

            var result = await _controller.Create(new CreateAdminActionRequest
            {
                ActionType = "Create", Details = "Создан товар", Branch = "Филиал 1"
            });

            Assert.IsType<OkResult>(result);
            _serviceMock.Verify(s => s.AddAction("Create", "Создан товар", "Филиал 1"), Times.Once);
        }
    }
}
