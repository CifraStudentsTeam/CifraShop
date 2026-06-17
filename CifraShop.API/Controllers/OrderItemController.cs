using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Requests.OrderItem;
using CifraShop.Contracts.Responses.OrderItem;
using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        //Конструктор
        public OrderItemController(IOrderItemService orderItemService, IOrderRepository orderRepository, IProductRepository productRepository)
        {
            orderItemService = _orderItemService;
            orderRepository = _orderRepository;
            productRepository = _productRepository;
        }

        //Получение состовляяющей заказа по id
        [HttpGet("/by-id")]
        public async Task<ActionResult<OrderItem>> GetOrderItemById([FromQuery] int id)
        {
            var orderItem = await _orderItemService.GetOrderItemById(id);

            if (orderItem == null) 
                return NotFound($"Состовляющея заказа с id {id} не найден");

            return Ok(orderItem);
        }

        //Получение состовляюющей заказа по id заказа
        [HttpGet("by-orderid")]
        public async Task<ActionResult<List<OrderItem>>> GetOrderItemsByOrderId([FromQuery] int id)
        {
            var orderItems = await _orderItemService.GetOrderItemsByOrderId(id);

            if (orderItems == null)
                return NotFound($"Состовляющие заказа с id заказа {id} не найден");

            return Ok(orderItems);
        }

        //Создание состовляющей заказа
        [HttpPost("/create-orderitem")]
        public async Task<ActionResult<OrderItem>> CreateOrderItem([FromBody] CreateOrderItemRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _orderRepository.GetOrderById(dto.OrderId);

            if (order == null)
                return BadRequest($"Заказ с id {dto.OrderId} не найден");

            var product = await _productRepository.GetProductsById(dto.ProductId);

            if (product == null)
                return BadRequest($"Продукт с id {dto.ProductId} не найден");

            await _orderItemService.CreateOrderItem(order, product, dto.Quantity);
            return CreatedAtAction(nameof(GetOrderItemById), new { orderItemId = 0 }, null);
        }

        [HttpPut("/update-orderitem")]
        public async Task<IActionResult> UpdateOrderItem([FromQuery] int id, [FromBody] UpdateOrderItemResponce dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.Id)
                return BadRequest("Несоответсвие id между URL-адресом и основным текстом");

            var existing = await _orderItemService.GetOrderItemById(id);

            if (existing == null)
                return NotFound($"Состовляющея заказа с id {id} не найден");

            existing.Quantity = dto.Quantity;

            if (dto.Price.HasValue)
                existing.Price = dto.Price.Value;

            await _orderItemService.UpdateOrderItem(existing);
            return NoContent();
        }

        [HttpDelete("/delete-orderItem")]
        public async Task<IActionResult> DeleteOrderItem([FromQuery] int id)
        {
            var existing = await _orderItemService.GetOrderItemById(id);

            if (existing == null)
                return NotFound($"Состовляющея заказа с id {id} не найден");

            await _orderItemService.DeleteOrderItem(existing);
            return NoContent(); 
        }
    }
}
