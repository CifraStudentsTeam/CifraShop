using CifraShop.API.Hubs;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Orders;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderImageRepository _imageRepository;
        private readonly IHubContext<AdminHub> _hub;

        public OrderController(IOrderService orderService, IOrderImageRepository imageRepository, IHubContext<AdminHub> hub)
        {
            _orderService = orderService;
            _imageRepository = imageRepository;
            _hub = hub;
        }

        private OrderResponse ToResponseWithImage(Domain.Entities.Order order)
        {
            var resp = order.ToResponse();
            var primaryImage = order.Images?.FirstOrDefault(i => i.IsPrimary)
                               ?? order.Images?.FirstOrDefault();
            if (primaryImage != null)
            {
                resp.ImageUrl = Url.Action("GetFile", "OrderImage", new { fileName = primaryImage.FileName })!;
            }
            return resp;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders.Select(o => ToResponseWithImage(o)).ToList());
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResponse<OrderResponse>>> GetPaged(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8,
            [FromQuery] string? search = null,
            [FromQuery] StatusOrder? status = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var paged = await _orderService.GetOrdersPaged(page, pageSize, search, status, dateFrom, dateTo);
            return Ok(new PagedResponse<OrderResponse>
            {
                Items = paged.Items.Select(o => ToResponseWithImage(o)).ToList(),
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            });
        }

        [HttpGet("by-id")]
        public async Task<ActionResult<OrderResponse>> GetById([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Заказ с id {id} не найден");
            return Ok(ToResponseWithImage(order));
        }

        [HttpGet("by-email")]
        public async Task<ActionResult<List<OrderResponse>>> GetByCustomerEmail(string email)
        {
            var orders = await _orderService.GetOrdersByCustomerEmail(email);
            return Ok(orders.Select(o => ToResponseWithImage(o)).ToList());
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<List<OrderResponse>>> GetByStatus(StatusOrder status)
        {
            var orders = await _orderService.GetOrdersByStatus(status);
            return Ok(orders.Select(o => ToResponseWithImage(o)).ToList());
        }

        [HttpGet("by-sum")]
        public async Task<ActionResult<List<OrderResponse>>> GetBySum([FromQuery] short sum)
        {
            var orders = await _orderService.GetOrdersBySum(sum);
            return Ok(orders.Select(o => ToResponseWithImage(o)).ToList());
        }

        [HttpGet("by-date")]
        public async Task<ActionResult<List<OrderResponse>>> GetByDate([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var orders = await _orderService.GetOrdersByDateRange(from, to);
            return Ok(orders.Select(o => ToResponseWithImage(o)).ToList());
        }

        [HttpPost("create-order")]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var items = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
            var order = await _orderService.CreateOrder(request.CustomerEmail, items);
            await _hub.Clients.All.SendAsync("Notify", "order", "created");
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order.ToResponse());
        }

        [HttpPut("update-order")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] UpdateOrderRequest request)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Заказ с id {id} не найден");

            if (request.Status.HasValue) order.Status = request.Status.Value;

            await _orderService.UpdateOrder(order);
            await _hub.Clients.All.SendAsync("Notify", "order", "updated");
            return NoContent();
        }

        [HttpPost("batch-update-status")]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchUpdateOrderStatusRequest request)
        {
            if (request.OrderIds == null || !request.OrderIds.Any())
                return BadRequest("Список id пуст");

            await _orderService.UpdateStatusRange(request.OrderIds, request.NewStatus);
            await _hub.Clients.All.SendAsync("Notify", "order", "updated");
            return Ok(new { updated = request.OrderIds.Count });
        }

        [HttpDelete("delete-order")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Заказ с id {id} не найден");

            await _orderService.DeleteOrder(order);
            await _hub.Clients.All.SendAsync("Notify", "order", "deleted");
            return NoContent();
        }
    }
}
