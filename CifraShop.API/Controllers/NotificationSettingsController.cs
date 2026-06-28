using CifraShop.API.Hubs;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.Notifications;
using CifraShop.Contracts.Responses.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class NotificationSettingsController : ControllerBase
    {
        private readonly INotificationSettingsService _service;
        private readonly IHubContext<AdminHub> _hub;

        public NotificationSettingsController(INotificationSettingsService service, IHubContext<AdminHub> hub)
        {
            _service = service;
            _hub = hub;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<NotificationSettingsResponse>>> GetAll()
        {
            var settings = await _service.GetAll();
            return Ok(settings.Select(MapToResponse).ToList());
        }

        [HttpGet("by-id")]
        public async Task<ActionResult<NotificationSettingsResponse>> GetById([FromQuery] int id)
        {
            var settings = await _service.GetById(id);
            if (settings == null) return NotFound($"Настройка с id {id} не найдена");
            return Ok(MapToResponse(settings));
        }

        [HttpGet("by-branch")]
        public async Task<ActionResult<NotificationSettingsResponse>> GetByBranch([FromQuery] string branch)
        {
            var settings = await _service.GetByBranch(branch);
            if (settings == null) return NotFound($"Настройка для филиала «{branch}» не найдена");
            return Ok(MapToResponse(settings));
        }

        [HttpPost]
        public async Task<ActionResult<NotificationSettingsResponse>> Create([FromBody] CreateNotificationSettingsRequest request)
        {
            var settings = await _service.Create(
                request.Email,
                request.Branch, request.NotifyOnNewOrder, request.NotifyOnStatusChange,
                request.NotifyOnLowStock, request.LowStockThreshold);
            settings.AdminEmails = request.AdminEmails;
            await _service.Update(settings);
            await _hub.Clients.All.SendAsync("Notify", "notification", "updated");
            return Ok(MapToResponse(settings));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] CreateNotificationSettingsRequest request)
        {
            var settings = await _service.GetById(id);
            if (settings == null) return NotFound($"Настройка с id {id} не найдена");

            settings.Email = request.Email;
            settings.Branch = request.Branch;
            settings.AdminEmails = request.AdminEmails;
            settings.NotifyOnNewOrder = request.NotifyOnNewOrder;
            settings.NotifyOnStatusChange = request.NotifyOnStatusChange;
            settings.NotifyOnLowStock = request.NotifyOnLowStock;
            settings.LowStockThreshold = request.LowStockThreshold;

            await _service.Update(settings);
            await _hub.Clients.All.SendAsync("Notify", "notification", "updated");
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var settings = await _service.GetById(id);
            if (settings == null) return NotFound($"Настройка с id {id} не найдена");
            await _service.Delete(settings);
            await _hub.Clients.All.SendAsync("Notify", "notification", "updated");
            return NoContent();
        }

        private static NotificationSettingsResponse MapToResponse(Domain.Entities.NotificationSettings s) => new()
        {
            Id = s.Id,
            Email = s.Email,
            Branch = s.Branch,
            AdminEmails = s.AdminEmails,
            NotifyOnNewOrder = s.NotifyOnNewOrder,
            NotifyOnStatusChange = s.NotifyOnStatusChange,
            NotifyOnLowStock = s.NotifyOnLowStock,
            LowStockThreshold = s.LowStockThreshold
        };
    }
}
