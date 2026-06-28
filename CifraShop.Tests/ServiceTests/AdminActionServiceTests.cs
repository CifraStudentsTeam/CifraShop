using CifraShop.Application.Services.Implementations;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Moq;

namespace CifraShop.Tests.ServiceTests
{
    public class AdminActionServiceTests
    {
        private readonly Mock<IAdminActionRepository> _repositoryMock;
        private readonly AdminActionService _service;

        public AdminActionServiceTests()
        {
            _repositoryMock = new Mock<IAdminActionRepository>();
            _service = new AdminActionService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetLastActions_ReturnsActions()
        {
            var actions = new List<AdminAction>
            {
                new() { Id = 1, ActionType = "Create", Details = "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1" },
                new() { Id = 2, ActionType = "Update", Details = "РћР±РЅРѕРІР»С‘РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1" }
            };
            _repositoryMock.Setup(r => r.GetLastActions(2, null)).ReturnsAsync(actions);

            var result = await _service.GetLastActions(2);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetLastActions_WithBranch_FiltersByBranch()
        {
            var actions = new List<AdminAction>
            {
                new() { Id = 1, Branch = "Р¤РёР»РёР°Р» 1" }
            };
            _repositoryMock.Setup(r => r.GetLastActions(10, "Р¤РёР»РёР°Р» 1")).ReturnsAsync(actions);

            var result = await _service.GetLastActions(10, "Р¤РёР»РёР°Р» 1");

            Assert.Single(result);
            Assert.Equal("Р¤РёР»РёР°Р» 1", result[0].Branch);
        }

        [Fact]
        public async Task GetLastActions_ZeroCount_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetLastActions(0));
        }

        [Fact]
        public async Task GetLastActions_NegativeCount_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetLastActions(-1));
        }

        [Fact]
        public async Task AddAction_ValidData_CreatesAction()
        {
            _repositoryMock.Setup(r => r.AddAction(It.IsAny<AdminAction>()))
                .Returns(Task.CompletedTask);

            await _service.AddAction("Create", "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", "Р¤РёР»РёР°Р» 1");

            _repositoryMock.Verify(r => r.AddAction(It.Is<AdminAction>(a =>
                a.ActionType == "Create" &&
                a.Details == "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ" &&
                a.Branch == "Р¤РёР»РёР°Р» 1" &&
                a.CreatedAt != default)), Times.Once);
        }

        [Fact]
        public async Task AddAction_NullType_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddAction(null!, "Details", "Branch"));
        }

        [Fact]
        public async Task AddAction_EmptyType_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddAction("", "Details", "Branch"));
        }

        [Fact]
        public async Task AddAction_NullDetails_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddAction("Create", null!, "Branch"));
        }

        [Fact]
        public async Task AddAction_EmptyDetails_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddAction("Create", "", "Branch"));
        }

        [Fact]
        public async Task AddAction_NullBranch_DefaultsEmpty()
        {
            _repositoryMock.Setup(r => r.AddAction(It.IsAny<AdminAction>()))
                .Returns(Task.CompletedTask);

            await _service.AddAction("Create", "Details", null);

            _repositoryMock.Verify(r => r.AddAction(It.Is<AdminAction>(a =>
                a.Branch == string.Empty)), Times.Once);
        }
    }
}
