using CifraShop.Domain.Enums;
using CifraShop.Domain.Models;
using CifraShop.Dto;
using CifraShop.Dto.Mappers;
using CIfraShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.Components.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
            => _productService = productService;

        [HttpGet]
        public async Task<ActionResult<List<ProductResponce>>> GetAll()
        {
            var products = await _productService.UploadingProductData();
            var response = products.Select(ProductMapper.ToResponse).ToList();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<ProductResponce>> GetById([FromQuery] int id)
        {
            var product = await _productService.GetProductsById((uint)id);

            if (product == null) 
                return NotFound($"Товар с  id {id} не найден");

            return Ok(ProductMapper.ToResponse(product));
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductResponce>>> GetByName([FromQuery] string name)
        {
            var products = await _productService.GetProductsByName(name);
            var responce = products.Select(ProductMapper.ToResponse).ToList();
            return Ok(responce);
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductResponce>>> GetByPrice([FromQuery] int price)
        {
            var products = await _productService.GetProductsByPrice((uint)price);
            var response = products.Select(ProductMapper.ToResponse).ToList();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductResponce>>> GetByQuantity([FromQuery] int quantity)
        {
            var products = await _productService.GetProductsByQuntity((uint)quantity);
            var response = products.Select(ProductMapper.ToResponse).ToList();
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductResponce>>> GetByStatus([FromQuery] string status)
        {
            if (!Enum.TryParse<StatusProduct>(status, out var statusEnum))
                return BadRequest($"Неверные данные. Выберите статус продукта: {string.Join(",", Enum.GetNames(typeof(StatusProduct)))}");
            var products = await _productService.GetProductsByStatus(statusEnum);
            var responce = products.Select(ProductMapper.ToResponse).ToList();
            return Ok(responce);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponce>> Create(CreateProductRequest request)
        {
            if (!Enum.TryParse<StatusProduct>(request.Status, true, out var status))
                return BadRequest($"Неверные данные. Выберите статус из списка: {string.Join(",", Enum.GetNames(typeof(StatusProduct)))}");

            var product = await _productService.CreateProduct(
                request.Name,
                request.Description,
                request.Price,
                request.Quantity,
                status
            );

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, ProductMapper.ToResponse(product));
        }

        [HttpPut]
        public async Task<ActionResult<ProductResponce>> ChangeName([FromQuery] int id, UpdateProductNameRequest request)
        {
            var product = await _productService.GetProductsById((uint)id);

            if (product == null)
                return NotFound($"Товар с id {id} не найден");

            await _productService.ChangeProductName(product, request.Name);
            return Ok(ProductMapper.ToResponse(product));
        }

        [HttpPut]
        public async Task<ActionResult<ProductResponce>> ChangePrice([FromQuery] int id, UpdateProductPriceRequest request)
        {
            var product = await _productService.GetProductsById((uint)id);

            if (product == null)
                return NotFound($"Товар с id {id} не найден");

            await _productService.ChangeProductPrice(product, request.Price);
            return Ok(ProductMapper.ToResponse(product));
        }

        [HttpGet]
        public async Task<ActionResult<ProductResponce>> ChangeQuantity([FromQuery] int id, UpdateProductQuantityRequest request)
        {
            var product = await _productService.GetProductsById((uint) id);

            if (product == null)
                return NotFound($"Товар с id {id} не найден");

            await _productService.ChangeProductQuntity(product, request.Quantity);
            return Ok(ProductMapper.ToResponse(product));
        }

        [HttpPut]
        public async Task<ActionResult<ProductResponce>> ChangeStatus(int id, UpdateProductStatusRequest request)
        {
            if (!Enum.TryParse<StatusProduct>(request.Status, true, out var status))
                return BadRequest($"Неверные данные. Введите статус из списка: {string.Join(",", Enum.GetNames(typeof(StatusProduct)))}");

            var product = await _productService.GetProductsById((uint)id);

            if (product == null)
                return NotFound($"Товар с id {id} не найден");

            await _productService.ChangeProductStatus(product, status);
            return Ok(ProductMapper.ToResponse(product));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var product = await _productService.GetProductsById((uint)id);

            if (product == null)
                return NotFound($"Товар с id {id} не найден");

            await _productService.DeleteProduct(product);
            return NoContent();
        }
    }
}
