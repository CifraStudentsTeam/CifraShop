using CifraShop.Domain.Models;
using CifraShop.Dto;
using CifraShop.Dto.Mappers;
using CIfraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Net.WebSockets;

namespace CifraShop.Components.Controllers
{
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;
        private readonly IOrderService _orderService;

        public OrderItemController(IOrderItemService orderItemService, IOrderService orderService)
        {
            _orderItemService = orderItemService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderItemResponce>>> GetOrderById([FromQuery] int orderId)
        {
            var items = await _orderItemService.GetOrderItemsByOrderId((uint)orderId);

            if (items == null || items.Any())
                return NotFound($"Товары в заказе: {orderId} не найдены");
            
            var resposce = items.Select(i => OrderItemMapper.ToResponse(i)).ToList();
            return Ok(resposce);
        }

        [HttpGet]
        public async Task<ActionResult<OrderItemResponce>> GetById([FromQuery] int id)
        {
            var item = await _orderItemService.GetOrderItemById((uint)id);

            if (item == null)
                return NotFound($"Товар в заказе с id {id} не найден");

            return Ok(OrderItemMapper.ToResponse(item));
        }

        [HttpPost]
        public async Task<ActionResult<OrderItemResponce>> Create([FromBody] AddOrderItemRequest request)
        {
            var order =  await _orderService.GetOrderById(request.OrderId);

            if (order == null)
                return BadRequest($"Заказ с id {request.OrderId} не найден");

            var newItem = new OrderItem
            {
                OrderId = request.OrderId,
                ProductId = request.ProdcutId,
                Quantity = request.Quantity,
                Price = request.Price,
            };

            await _orderItemService.AddOrderItem(newItem);
            return CreatedAtAction(nameof(GetById), new { id = newItem.Id }, OrderItemMapper.ToResponse(newItem));
        }

        [HttpPut]
        public async Task<ActionResult<OrderItemResponce>> Update([FromQuery] int id, [FromBody] UpdateOrderItemRequest request)
        {
            var existing =  await _orderItemService.GetOrderItemById((uint) id);

            if (existing == null)
                return NotFound($"Товар с id {id} в заказе не найден ");

            existing.OrderId = request.OrderId;
            existing.ProductId = request.ProductId;
            existing.Quantity = request.Quantity;
            existing.Price = request.Price;

            await _orderItemService.UpdateOrderItem(existing);
            return Ok(OrderItemMapper.ToResponse(existing));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var item = await _orderItemService.GetOrderItemById((uint)id);

            if (item == null)
                return NotFound($"Товар с id {id} в заказе не найден");

            await _orderItemService.RemoveOrderItem(item);
            return NoContent();
        } 
    }
}
