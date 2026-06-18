using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Products;
using CifraShop.Contracts.Responses.Products;
using CifraShop.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("all")]
        public async Task<ActionResult<List<ProductResponse>>> GetAllProduct()
        {
            var products = await _productService.GetAllProducts();
            var result = new List<ProductResponse>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        [HttpGet("by-id")]
        public async Task<ActionResult<ProductResponse>> GetProductById([FromQuery] int id)
        {
            var product = await _productService.GetProductById(id);

            if (product == null)
                return NotFound($"Продукт с id {id} не найден");

            return Ok(product.ToResponse());
        }

        [HttpGet("by-name")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByName([FromQuery] string name)
        {
            var products = await _productService.GetProductsByName(name);
            var result = new List<ProductResponse>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        [HttpGet("by-price")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByPrice([FromQuery] short price)
        {
            var products = await _productService.GetProductsByPrice(price);
            var result = new List<ProductResponse>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        [HttpGet("by-quantity")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByQuantity([FromQuery] short quantity)
        {
            var products = await _productService.GetProductsByQuantity(quantity);
            var result = new List<ProductResponse>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByStatus(StatusProduct statusProduct)
        {
            var products = await _productService.GetProductsByStatus(statusProduct);
            var result = new List<ProductResponse>();

            foreach (var product in products)
                result.Add(product.ToResponse());

            return Ok(result);
        }

        [HttpPost("create-product")]
        public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProduct(request.Name, request.Description, request.Price, request.Quantity);
            return Ok(product.ToResponse());
        }

        [HttpPut("update-product")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request, [FromQuery] int id)
        {
            var product = await _productService.GetProductById(id);

            if (product == null)
                return NotFound($"Продукт с id {id} не найден");

            if (request.Name != null)
                product.Name = request.Name;

            if (request.Description != null)
                product.Description = request.Description;

            if (request.Price.HasValue)
                product.Price = request.Price.Value;

            if (request.Quantity.HasValue)
                product.Quantity = request.Quantity.Value;

            if (request.Status.HasValue)
                product.Status = (StatusProduct)request.Status.Value;

            await _productService.UpdateProduct(product);
            return NoContent();
        }

        [HttpDelete("delete-product")]
        public async Task<IActionResult> DeleteProduct([FromQuery] int id)
        {
            var product = await _productService.GetProductById(id);

            if (product == null)
                return NotFound($"Продукт с id {id} не найден");

            await _productService.DeleteProduct(product);
            return NoContent();
        }
    }
}
