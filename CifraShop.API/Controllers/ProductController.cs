using CifraShop.Application.Services.Implementations;
using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Products;
using CifraShop.Contracts.Responses.Products;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        //Получение всех продуктов
        [HttpGet("all")]
        public async Task<ActionResult<List<ProductResponce>>> GetAllProduct()
        {
            var products = await _productService.GetAllProducts();
            var result = new List<ProductResponce>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        //Получение продукта по id
        [HttpGet("by-id")]
        public async Task<ActionResult<List<ProductResponce>>> GetProductsById([FromQuery] int id)
        {
            var product = await _productService.GetProductsById(id);

            if (product == null)
                return NotFound($"Продукт с id {id} не найден");

            return Ok(product.ToResponse());
        }

        //Получение продуктов по имени
        [HttpGet("by-name")]
        public async Task<ActionResult<List<ProductResponce>>> GetProductsByName([FromQuery] string name)
        {
            var products = await _productService.GetProductsByName(name);
            var result = new List<ProductResponce>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        //Получение продуктов по цене 
        [HttpGet("by-price")]
        public async Task<ActionResult<List<ProductResponce>>> GetProductsByPrice([FromQuery] short price)
        {
            var products = await _productService.GetProductsByPrice(price);
            var result = new List<ProductResponce>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        //Получение продуктов по количеству 
        [HttpGet("by-quantity")]
        public async Task<ActionResult<List<ProductResponce>>> GetProductsByQuantity([FromQuery] short quantity)
        {
            var products = await _productService.GetProductsByQuntity(quantity);
            var result = new List<ProductResponce>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        //Получение продуктов по статусу
        [HttpGet("by-status")]
        public async Task<ActionResult<List<ProductResponce>>> GetProductsByStatus(StatusProduct statusProduct)
        {
            var products = await _productService.GetProductsByStatus(statusProduct);
            var result = new List<ProductResponce>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        //Создание продукта
        [HttpPost("create-product")]
        public async Task<ProductResponce> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProductData(request.Name, request.Description, request.Price, request.Quantity);
            //if (product == null)
            //    return StatusCode(500, "Заказ не был создан");
            return product.ToResponse();
        }

        //Обновление продукта 
        [HttpPut("update-product")]
        public async Task<IActionResult> UpdateProduct([FromBody] UppdateProductRequest request, [FromQuery] int id)
        {
            var product = await _productService.GetProductsById(id);

            if (product == null)
                return NotFound($"Продукт  с id {id} не найден");

            if (request.Name !=null)
                product.Name = request.Name;

            if (request.Description !=null)
                product.Description = request.Description;

            if (request.Price.HasValue)
                product.Price = request.Price.Value;

            if (request.Quantity.HasValue)
                product.Quantity = request.Quantity.Value;

            if (request.Status.HasValue)
                product.Status = (Domain.Enums.StatusProduct)request.Status.Value;
           
            await _productService.UpdateProduct(product);
            return NoContent();
        }

        //Удаление продукта 
        [HttpDelete("delete-product")]
        public async Task<IActionResult> DeleteProduct([FromQuery] int id)
        {
            var product = await _productService.GetProductsById(id);

            if (product == null)
                return NotFound($"Продукт с id {id} не найден");

            await _productService.DeleteProduct(product);
            return NoContent();
        }
    }
}
