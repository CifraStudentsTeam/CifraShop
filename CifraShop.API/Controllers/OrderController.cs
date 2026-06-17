using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Orders;
using CifraShop.Contracts.Responses.Orders;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserRepository _userRepository;
        private readonly IOrderItemService _orderItemService;
        private readonly IProductRepository _productRepository;

        //Конструктор
        public OrderController(IOrderService orderService, IUserRepository userRepository, IOrderItemService orderItemService, IProductRepository productRepository)
        {
            _orderService = orderService;
            _userRepository = userRepository;
            _orderItemService = orderItemService;
            _productRepository = productRepository;
        }

        //Получение всех заказов
        [HttpGet("/all")]
        public async Task<ActionResult<List<OrderResponse>>> GetAll()
        {
            var orders = await _orderService.GetAllOrder();
            var result = new List<OrderResponse>();

            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponce(items));
            }

            return Ok(result);
        }

        //Получение заказа по id
        [HttpGet("/by-id")]
        public async Task<ActionResult<OrderResponse>> GetById([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById(id);

            if (order == null)
                return NotFound($"Заказ с id {id} не найден");

            var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
            return Ok(order.ToResponce(items));
        }

        //Получение заказа по логину
        [HttpGet("/by-login")]
        public async Task<ActionResult<List<OrderResponse>>> GetByCustomerLogin(string login)
        {
            var orders = await _orderService.GetOrdersByCustomerLogin(login);
            var result = new List<OrderResponse>();

            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponce(items));
            }
            return Ok(result);
        }

        //Получение заказа по статусу 
        [HttpGet("by-status")]
        public async Task<ActionResult<List<OrderResponse>>> GetByStatus(StatusOrder status)
        {
            var orders = await _orderService.GetOrdersByStatus(status);
            var result = new List<OrderResponse>();

            foreach(var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponce(items));
            }

            return Ok(result);
        }


        //Получение заказа по сумме
        [HttpGet("by-sum")]
        public async Task<ActionResult<List<OrderResponse>>> GetBySum([FromQuery] short sum)
        {
            var orders = await _orderService.GetOrdersBySum(sum);
            var result = new List<OrderResponse>();

            foreach (var order in orders)
            {
                var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);
                result.Add(order.ToResponce(items));
            }

            return Ok(result);
        }

        //Создание заказа
        [HttpPost("create-order")]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetUserByEmail(request.CustomerLogin);

            if (user == null)
                return BadRequest($"Пользователь с логином {request.CustomerLogin} не найден");

            int totalSum = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemReq in request.Items)
            {
                var product = await _productRepository.GetProductsById(itemReq.ProductId);

                if (product == null)
                    return BadRequest($"Продукт с id {itemReq.ProductId} не найден");

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductInOrder = product,
                    Price = product.Price,
                    Quantity = product.Quantity
                };

                orderItems.Add(orderItem);
                totalSum += product.Price * itemReq.Quantity;
            }

            var createdOrder = await _orderService.CreateOrder((short)totalSum, user, orderItems);

            if (createdOrder == null)
                return StatusCode(500, "Заказ не был создан");

            foreach (var item in orderItems)
                await _orderItemService.CreateOrderItem(createdOrder, item.ProductInOrder, item.Quantity);

            var finalItems = await _orderItemService.GetOrderItemsByOrderId(createdOrder.Id);
            var responce = createdOrder.ToResponce(finalItems);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, responce);
        }

        //Обновление заказа
        [HttpPut("update-order")]
        public async Task<IActionResult> Update([FromQuery] int id, [FromBody] UpdateOrderRequest request)
        {
            var order = await _orderService.GetOrderById(id);

            if (order == null)
                return NotFound($"Заказ с id {id} не был найден");

            if (request.Status.HasValue)
                order.Status = request.Status.Value;

            if (request.Sum.HasValue)
                order.Sum = request.Sum.Value;

            await _orderService.UpdateOrder(order);
            return NoContent();
        }

        //Удаление заказа
        [HttpDelete("delete-order")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById(id);

            if (order == null)
                return NotFound();

            var items = await _orderItemService.GetOrderItemsByOrderId(order.Id);

            foreach (var item in items)
                await _orderItemService.DeleteOrderItem(item);

            await _orderService.DeleteOrder(order);
            return NoContent();
        }
    }
}
