using CifraShop.Domain.Enums;
using CifraShop.Dto;
using CifraShop.Dto.Mappers;
using CIfraShop.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.Components.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
            => _orderService = orderService;

        [HttpGet]
        public async Task<ActionResult<List<OrderResponce>>> GetAll()
        {
            var orders = await _orderService.UploadingOrderData();
            var response = orders.Select(OrderMapper.ToResponce).ToList();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<OrderResponce>> GetById([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById((uint)id);

            if (order == null)
                return NotFound($"Заказ с id {id} не найден");

            return Ok(OrderMapper.ToResponce(order));
        }

        [HttpGet]
        public async Task<ActionResult<OrderResponce>> GetByLogin([FromQuery] string login)
        {
            var orders = await _orderService.GetOrdersByLogin(login);
            var responce = orders.Select(OrderMapper.ToResponce).ToList();
            return Ok(responce);
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderResponce>>> GetByStatus([FromQuery] string status)
        {
            if (!Enum.TryParse<StatusOrder>(status, true, out var statusEnum))
                return BadRequest($"Неверные данные. Введите статус из списка: { string.Join(", ", Enum.GetNames<StatusOrder>())}");

            var orders = await _orderService.GetOrdersByStatus(statusEnum);
            var responce = orders.Select(OrderMapper.ToResponce).ToList();
            return Ok(responce);
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderResponce>>> GetBySum([FromQuery] int sum)
        {
            var orders = await _orderService.GetOrdersBySum((uint)sum);
            var responce = orders.Select(OrderMapper.ToResponce).ToList().ToList();
            return Ok(responce);
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponce>> Create(CreateOrderRequest request)
        {
            if (!Enum.TryParse<StatusOrder>(request.Status, true, out var status))
                return BadRequest($"Неверные данные. Введите стаус из списка: {string.Join(",", Enum.GetNames<StatusOrder>())}");

            var order = await _orderService.CreateOrder(status, request.Sum, request.CustomerLogin);
            return CreatedAtAction(nameof(GetById), new { Id = order.Id }, OrderMapper.ToResponce(order));
        }

        [HttpPut]
        public async Task<ActionResult<OrderResponce>> ChangeStatus([FromQuery] int id, ChangeOrderStatusRequest request)
        {
            var order = await _orderService.GetOrderById((uint)id);

            if (order == null)
                return NotFound($"Заказ с id {id} не найден");

            if (!Enum.TryParse<StatusOrder>(request.NewStatus, true, out var newStatus))
                return BadRequest($"Неверные данные. Введите статус из списка: {string.Join(" ,", Enum.GetNames<StatusOrder>())})");
            var updatedOrder = await _orderService.ChangeOrderStatus(order, newStatus);
            return Ok (OrderMapper.ToResponce(updatedOrder));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById((uint)id);
            if (order == null)
                return NotFound($"Order with id {id} not found.");
            await _orderService.DeleteOrder(order);
            return NoContent();
        }
    }
}
     