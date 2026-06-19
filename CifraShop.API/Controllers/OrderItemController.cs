using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.OrderItem;
using CifraShop.Contracts.Responses.OrderItem;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet("by-id")]
        public async Task<ActionResult<OrderItemResponse>> GetOrderItemById([FromQuery] int id)
        {
            var orderItem = await _orderItemService.GetOrderItemById(id);
            if (orderItem == null)
                return NotFound($"Составляющая заказа с id {id} не найдена");
            return Ok(orderItem.ToResponse());
        }

        [HttpGet("by-orderid")]
        public async Task<ActionResult<List<OrderItemResponse>>> GetOrderItemsByOrderId([FromQuery] int id)
        {
            var orderItems = await _orderItemService.GetOrderItemsByOrderId(id);
            return Ok(orderItems.Select(i => i.ToResponse()).ToList());
        }

        [HttpPost("create-orderitem")]
        public async Task<ActionResult<OrderItemResponse>> CreateOrderItem([FromBody] CreateOrderItemRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var orderItem = await _orderItemService.CreateOrderItem(dto.OrderId, dto.ProductId, dto.Quantity);
            return CreatedAtAction(nameof(GetOrderItemById), new { id = orderItem.Id }, orderItem.ToResponse());
        }

        [HttpPut("update-orderitem")]
        public async Task<IActionResult> UpdateOrderItem([FromQuery] int id, [FromBody] UpdateOrderItemRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("Идентификатор из URL-параметра не совпадает с телом запроса");

            var existing = await _orderItemService.GetOrderItemById(id);
            if (existing == null)
                return NotFound($"Составляющая заказа с id {id} не найдена");

            existing.Quantity = dto.Quantity;
            if (dto.Price.HasValue)
                existing.Price = dto.Price.Value;

            await _orderItemService.UpdateOrderItem(existing);
            return NoContent();
        }

        [HttpDelete("delete-order-item")]
        public async Task<IActionResult> DeleteOrderItem([FromQuery] int id)
        {
            var existing = await _orderItemService.GetOrderItemById(id);
            if (existing == null)
                return NotFound($"Составляющая заказа с id {id} не найдена");

            await _orderItemService.DeleteOrderItem(existing);
            return NoContent();
        }
    }
}
