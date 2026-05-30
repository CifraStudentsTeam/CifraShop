using CifraShop.Domain.Models;
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
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            var products = await _productService.UploadingProductData();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _productService.GetProductsById((uint)id);
            if (product == null)
                return NotFound($"Product with id {id} not found.");
            return Ok(product);
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<List<Product>>> GetProductsByName(string name)
        {
            var products = _productService.GetProductsByName(name);
            return Ok(products);
        }

        [HttpGet("price/{price:int}")]
        public async Task<ActionResult<List<Product>>> GetProductsByPrice(int price)
        {
            var products = await _productService.GetProductsByPrice((uint)price);
            return Ok(products);
        }

        //[HttpGet("quantity/{quantity:uint}")] 
        //public async Task<ActionResult<List<Product>>> GetProductByQuantity(uint quantity)
        //{
        //    var products = await _productService.GetProductsByQuntity(quantity);
        //}
    }
}
