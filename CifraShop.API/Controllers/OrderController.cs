using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Orders;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
            => _orderService = orderService;

        [HttpGet("all")]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders.Select(o => o.ToResponse()).ToList());
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
                Items = paged.Items.Select(o => o.ToResponse()).ToList(),
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
            return Ok(order.ToResponse());
        }

        [HttpGet("by-email")]
        public async Task<ActionResult<List<OrderResponse>>> GetByCustomerEmail(string email)
        {
            var orders = await _orderService.GetOrdersByCustomerEmail(email);
            return Ok(orders.Select(o => o.ToResponse()).ToList());
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<List<OrderResponse>>> GetByStatus(StatusOrder status)
        {
            var orders = await _orderService.GetOrdersByStatus(status);
            return Ok(orders.Select(o => o.ToResponse()).ToList());
        }

        [HttpGet("by-sum")]
        public async Task<ActionResult<List<OrderResponse>>> GetBySum([FromQuery] short sum)
        {
            var orders = await _orderService.GetOrdersBySum(sum);
            return Ok(orders.Select(o => o.ToResponse()).ToList());
        }

        [HttpGet("by-date")]
        public async Task<ActionResult<List<OrderResponse>>> GetByDate([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var orders = await _orderService.GetOrdersByDateRange(from, to);
            return Ok(orders.Select(o => o.ToResponse()).ToList());
        }

        [HttpPost("create-order")]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var items = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
            var order = await _orderService.CreateOrder(request.CustomerEmail, items);

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order.ToResponse());
        }

        [HttpPut("update-order")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] UpdateOrderRequest request)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Заказ с id {id} не найден");

            if (request.Status.HasValue) order.Status = request.Status.Value;

            await _orderService.UpdateOrder(order);
            return NoContent();
        }

        [HttpPost("batch-update-status")]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchUpdateOrderStatusRequest request)
        {
            if (request.OrderIds == null || !request.OrderIds.Any())
                return BadRequest("Список id пуст");

            await _orderService.UpdateStatusRange(request.OrderIds, request.NewStatus);
            return Ok(new { updated = request.OrderIds.Count });
        }

        [HttpDelete("delete-order")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Заказ с id {id} не найден");

            await _orderService.DeleteOrder(order);
            return NoContent();
        }
    }
}
