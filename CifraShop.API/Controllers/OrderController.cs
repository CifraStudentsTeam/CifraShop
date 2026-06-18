using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Orders;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderItemService _orderItemService;
        private readonly IUserService _userService;

        public OrderController(IOrderService orderService, IOrderItemService orderItemService, IUserService userService)
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
            _userService = userService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            var orders = await _orderService.GetAllOrders();
            var result = new List<OrderResponse>();
            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponse(items));
            }
            return Ok(result);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResponse<OrderResponse>>> GetPaged([FromQuery] int page = 0, [FromQuery] int pageSize = 8)
        {
            var paged = await _orderService.GetOrdersPaged(page, pageSize);
            var result = new List<OrderResponse>();
            foreach (var order in paged.Items)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponse(items));
            }
            return Ok(new PagedResponse<OrderResponse>
            {
                Items = result,
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
            var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
            return Ok(order.ToResponse(items));
        }

        [HttpGet("by-login")]
        public async Task<ActionResult<List<OrderResponse>>> GetByCustomerLogin(string login)
        {
            var orders = await _orderService.GetOrdersByCustomerLogin(login);
            var result = new List<OrderResponse>();
            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponse(items));
            }
            return Ok(result);
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<List<OrderResponse>>> GetByStatus(StatusOrder status)
        {
            var orders = await _orderService.GetOrdersByStatus(status);
            var result = new List<OrderResponse>();
            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponse(items));
            }
            return Ok(result);
        }

        [HttpGet("by-sum")]
        public async Task<ActionResult<List<OrderResponse>>> GetBySum([FromQuery] short sum)
        {
            var orders = await _orderService.GetOrdersBySum(sum);
            var result = new List<OrderResponse>();
            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponse(items));
            }
            return Ok(result);
        }

        [HttpGet("by-date")]
        public async Task<ActionResult<List<OrderResponse>>> GetByDate([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var orders = await _orderService.GetOrdersByDateRange(from, to);
            var result = new List<OrderResponse>();
            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponse(items));
            }
            return Ok(result);
        }

        [HttpPost("create-order")]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userService.GetUserByEmail(request.CustomerLogin);
            if (user == null) return BadRequest($"Пользователь с логином {request.CustomerLogin} не найден");

            var createdOrder = await _orderService.CreateOrder(0, user.Id, user.Email, new List<OrderItem>());
            if (createdOrder == null) return StatusCode(500, "Заказ не был создан");

            short totalSum = 0;
            foreach (var itemReq in request.Items)
            {
                var orderItem = await _orderItemService.CreateOrderItem(createdOrder.Id, itemReq.ProductId, itemReq.Quantity);
                totalSum += (short)(orderItem.Price * itemReq.Quantity);
            }

            createdOrder.Sum = totalSum;
            await _orderService.UpdateOrder(createdOrder);

            var finalItems = await _orderItemService.GetOrderItemsByOrderId(createdOrder.Id);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder.ToResponse(finalItems));
        }

        [HttpPut("update-order")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] UpdateOrderRequest request)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound($"Заказ с id {id} не найден");

            if (request.Status.HasValue) order.Status = request.Status.Value;
            if (request.Sum.HasValue) order.Sum = request.Sum.Value;

            await _orderService.UpdateOrder(order);
            return NoContent();
        }

        [HttpPost("batch-update-status")]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchUpdateOrderStatusRequest request)
        {
            if (request.OrderIds == null || !request.OrderIds.Any())
                return BadRequest("Список id пуст");

            var status = (StatusOrder)request.NewStatus;
            await _orderService.UpdateStatusRange(request.OrderIds, status);
            return Ok(new { updated = request.OrderIds.Count });
        }

        [HttpDelete("delete-order")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
            foreach (var item in items)
                await _orderItemService.DeleteOrderItem(item);

            await _orderService.DeleteOrder(order);
            return NoContent();
        }
    }
}
