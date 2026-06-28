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

        // РРЅРёС†РёР°Р»РёР·РёСЂСѓРµС‚ РјРѕРє IAdminActionService Рё СЃРѕР·РґР°С‘С‚ СЌРєР·РµРјРїР»СЏСЂ AdminActionController РїРµСЂРµРґ РєР°Р¶РґС‹Рј С‚РµСЃС‚РѕРј.
        public AdminActionControllerTests()
        {
            _serviceMock = new Mock<IAdminActionService>();
            _controller = new AdminActionController(_serviceMock.Object);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РјРµС‚РѕРґ GetLast РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°СѓСЃ 200 OK Рё СЃРїРёСЃРѕРє РёР· РґРІСѓС… РґРµР№СЃС‚РІРёР№ Р°РґРјРёРЅРёСЃС‚СЂР°С‚РѕСЂР°.
        public async Task CheckingGetLastReturnsOKWith2Actions()
        {
            var actions = new List<AdminAction>
            {
                new() { Id = 1, ActionType = "Create", Details = "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = DateTime.UtcNow },
                new() { Id = 2, ActionType = "Update", Details = "РћР±РЅРѕРІР»С‘РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = DateTime.UtcNow }
            };
            _serviceMock.Setup(s => s.GetLastActions(50, null)).ReturnsAsync(actions);

            var result = await _controller.GetLast();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.AdminAction.AdminActionResponse>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ GetLast СЃ РїР°СЂР°РјРµС‚СЂРѕРј С„РёР»РёР°Р»Р° РІРѕР·РІСЂР°С‰Р°РµС‚ С‚РѕР»СЊРєРѕ РґРµР№СЃС‚РІРёСЏ СЌС‚РѕРіРѕ С„РёР»РёР°Р»Р°.
        public async Task CheckingFilteredByBranchReturnsSingleAction()
        {
            var actions = new List<AdminAction>
            {
                new() { Id = 1, ActionType = "Create", Details = "Details", Branch = "Р¤РёР»РёР°Р» 1", CreatedAt = DateTime.UtcNow }
            };
            _serviceMock.Setup(s => s.GetLastActions(10, "Р¤РёР»РёР°Р» 1")).ReturnsAsync(actions);

            var result = await _controller.GetLast(10, "Р¤РёР»РёР°Р» 1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.AdminAction.AdminActionResponse>>(okResult.Value);
            Assert.Single(list);
            Assert.Equal("Р¤РёР»РёР°Р» 1", list[0].Branch);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РѕС‚СЃСѓС‚СЃС‚РІРёРё РґРµР№СЃС‚РІРёР№ РІРѕР·РІСЂР°С‰Р°РµС‚СЃСЏ СЃС‚Р°С‚СѓСЃ 200 OK СЃ РїСѓСЃС‚С‹Рј СЃРїРёСЃРєРѕРј.
        public async Task CheckingEmptyListReturnsOKWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetLastActions(50, null)).ReturnsAsync(new List<AdminAction>());

            var result = await _controller.GetLast();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<List<Contracts.Responses.AdminAction.AdminActionResponse>>(okResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        // РџСЂРѕРІРµСЂРєР°, С‡С‚Рѕ РїСЂРё РІР°Р»РёРґРЅС‹С… РґР°РЅРЅС‹С… СЃРѕР·РґР°РЅРёРµ РґРµР№СЃС‚РІРёСЏ РІРѕР·РІСЂР°С‰Р°РµС‚ СЃС‚Р°С‚СѓСЃ 200 OK Рё РІС‹Р·С‹РІР°РµС‚ AddAction СЃ РєРѕСЂСЂРµРєС‚РЅС‹РјРё РїР°СЂР°РјРµС‚СЂР°РјРё.
        public async Task CheckingValidCreationReturnsOKAndCallsAddActionOnce()
        {
            _serviceMock.Setup(s => s.AddAction("Create", "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", "Р¤РёР»РёР°Р» 1"))
                .Returns(Task.CompletedTask);

            var result = await _controller.Create(new CreateAdminActionRequest
            {
                ActionType = "Create", Details = "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", Branch = "Р¤РёР»РёР°Р» 1"
            });

            Assert.IsType<OkResult>(result);
            _serviceMock.Verify(s => s.AddAction("Create", "РЎРѕР·РґР°РЅ С‚РѕРІР°СЂ", "Р¤РёР»РёР°Р» 1"), Times.Once);
        }
    }
}
