using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Mappings;
using CifraShop.Contracts.Requests.Products;
using CifraShop.Contracts.Responses.Common;
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

        private ProductResponse ToResponseWithUrl(Domain.Entities.Product p)
        {
            var resp = p.ToResponse();
            if (!string.IsNullOrEmpty(resp.ImageUrl) && !resp.ImageUrl.StartsWith("http"))
            {
                resp.ImageUrl = Url.Action("GetFile", "ProductImage", new { fileName = resp.ImageUrl })!;
            }
            return resp;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<ProductResponse>>> GetAllProducts()
        {
            var products = await _productService.GetAllProducts();
            return Ok(products.Select(p => ToResponseWithUrl(p)).ToList());
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResponse<ProductResponse>>> GetPaged(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 8,
            [FromQuery] string? search = null,
            [FromQuery] StatusProduct? status = null)
        {
            var paged = await _productService.GetProductsPaged(page, pageSize, search, status);
            return Ok(new PagedResponse<ProductResponse>
            {
                Items = paged.Items.Select(p => ToResponseWithUrl(p)).ToList(),
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            });
        }

        [HttpGet("by-id")]
        public async Task<ActionResult<ProductResponse>> GetProductById([FromQuery] int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound($"Товар с id {id} не найден");
            return Ok(ToResponseWithUrl(product));
        }

        [HttpGet("by-name")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByName([FromQuery] string name)
        {
            var products = await _productService.GetProductsByName(name);
            return Ok(products.Select(p => ToResponseWithUrl(p)).ToList());
        }

        [HttpGet("by-price")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByPrice([FromQuery] short price)
        {
            var products = await _productService.GetProductsByPrice(price);
            return Ok(products.Select(p => ToResponseWithUrl(p)).ToList());
        }

        [HttpGet("by-quantity")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByQuantity([FromQuery] short quantity)
        {
            var products = await _productService.GetProductsByQuantity(quantity);
            return Ok(products.Select(p => ToResponseWithUrl(p)).ToList());
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsByStatus(StatusProduct statusProduct)
        {
            var products = await _productService.GetProductsByStatus(statusProduct);
            return Ok(products.Select(p => ToResponseWithUrl(p)).ToList());
        }

        [HttpPost("create-product")]
        public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProduct(request.Name, request.Description, request.Price, request.Quantity);
            return Ok(ToResponseWithUrl(product));
        }

        [HttpPut("update-product")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request, [FromQuery] int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound($"Товар с id {id} не найден");

            if (request.Name != null) product.Name = request.Name;
            if (request.Description != null) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.Quantity.HasValue) product.Quantity = request.Quantity.Value;
            if (request.Status.HasValue) product.Status = request.Status.Value;

            await _productService.UpdateProduct(product);
            return NoContent();
        }

        [HttpDelete("delete-product")]
        public async Task<IActionResult> DeleteProduct([FromQuery] int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound($"Товар с id {id} не найден");
            await _productService.DeleteProduct(product);
            return NoContent();
        }

        [HttpPost("batch-delete")]
        public async Task<IActionResult> BatchDelete([FromBody] BatchDeleteProductsRequest request)
        {
            if (request.ProductIds == null || !request.ProductIds.Any())
                return BadRequest("Список id пуст");

            var existingIds = new List<int>();
            foreach (var id in request.ProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null) existingIds.Add(id);
            }

            if (!existingIds.Any())
                return NotFound("Ни один из указанных товаров не найден");

            await _productService.DeleteRange(existingIds);
            return Ok(new { deleted = existingIds.Count, notFound = request.ProductIds.Count - existingIds.Count });
        }

        [HttpPost("batch-update-status")]
        public async Task<IActionResult> BatchUpdateStatus([FromBody] BatchUpdateProductStatusRequest request)
        {
            if (request.ProductIds == null || !request.ProductIds.Any())
                return BadRequest("Список id пуст");

            await _productService.UpdateStatusRange(request.ProductIds, request.NewStatus);
            return Ok(new { updated = request.ProductIds.Count });
        }
    }
}
