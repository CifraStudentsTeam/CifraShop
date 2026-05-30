using CifraShop.Dto;
using CifraShop.Dto.Mappers;
using CIfraShop.Services.Interfaces;
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
    }
}
     