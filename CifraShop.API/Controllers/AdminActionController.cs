using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.AdminAction;
using CifraShop.Contracts.Responses.AdminAction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminActionController : ControllerBase
    {
        private readonly IAdminActionService _service;

        public AdminActionController(IAdminActionService service)
            => _service = service;

        [HttpGet("last")]
        public async Task<ActionResult<List<AdminActionResponse>>> GetLast([FromQuery] int count = 50, [FromQuery] string? branch = null)
        {
            await _service.TrimOldActions(1000);
            var actions = await _service.GetLastActions(count, branch);
            var result = actions.Select(a => new AdminActionResponse
            {
                Id = a.Id,
                ActionType = a.ActionType,
                Details = a.Details,
                Branch = a.Branch,
                CreatedAt = a.CreatedAt
            }).ToList();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdminActionRequest request)
        {
            await _service.AddAction(request.ActionType, request.Details, request.Branch);
            return Ok();
        }
    }
}
