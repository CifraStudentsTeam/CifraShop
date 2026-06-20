using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageRepository _imageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        public ProductImageController(
            IProductImageRepository imageRepository,
            IProductRepository productRepository,
            IWebHostEnvironment env)
        {
            _imageRepository = imageRepository;
            _productRepository = productRepository;
            _env = env;
        }

        [HttpGet("by-product")]
        public async Task<IActionResult> GetByProductId([FromQuery] int productId)
        {
            var images = await _imageRepository.GetByProductId(productId);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = images.Select(i => new
            {
                i.Id,
                Url = $"{baseUrl}{Url.Action(nameof(GetFile), new { fileName = i.FileName })}",
                i.FileName,
                i.IsPrimary,
                i.SortOrder
            });
            return Ok(result);
        }

        [HttpGet("file/{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            var filePath = Path.Combine(_env.WebRootPath, "images", "products", fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] int productId, [FromForm] IFormFile file, [FromForm] bool isPrimary = false)
        {
            var product = await _productRepository.GetProductById(productId);
            if (product == null) return NotFound($"Товар с id {productId} не найден");

            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Максимальный размер файла — 5 МБ");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest("Допустимые форматы: jpg, jpeg, png, webp, gif");

            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{productId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var count = await _imageRepository.GetCountByProductId(productId);

            var image = new ProductImage
            {
                ProductId = productId,
                FileName = fileName,
                IsPrimary = isPrimary || count == 0,
                SortOrder = count
            };

            await _imageRepository.Add(image);

            if (image.IsPrimary)
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                product.ImageUrl = $"{baseUrl}{Url.Action(nameof(GetFile), new { fileName })}";
                await _productRepository.UpdateProduct(product);
            }

            var fullUrl = $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetFile), new { fileName })}";
            return Ok(new { image.Id, url = fullUrl, image.FileName, image.IsPrimary, image.SortOrder });
        }

        [HttpPost("set-primary")]
        public async Task<IActionResult> SetPrimary([FromQuery] int imageId)
        {
            var image = await _imageRepository.GetById(imageId);
            if (image == null) return NotFound();

            var allImages = await _imageRepository.GetByProductId(image.ProductId);
            foreach (var img in allImages)
            {
                if (img.Id == imageId && !img.IsPrimary)
                {
                    img.IsPrimary = true;
                    await _imageRepository.Update(img);
                }
                else if (img.Id != imageId && img.IsPrimary)
                {
                    img.IsPrimary = false;
                    await _imageRepository.Update(img);
                }
            }

            var product = await _productRepository.GetProductById(image.ProductId);
            if (product != null)
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                product.ImageUrl = $"{baseUrl}{Url.Action(nameof(GetFile), new { fileName = image.FileName })}";
                await _productRepository.UpdateProduct(product);
            }

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var image = await _imageRepository.GetById(id);
            if (image == null) return NotFound();

            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
            var filePath = Path.Combine(uploadsDir, image.FileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var wasPrimary = image.IsPrimary;
            var productId = image.ProductId;

            await _imageRepository.Delete(image);

            if (wasPrimary)
            {
                var remaining = await _imageRepository.GetByProductId(productId);
                var product = await _productRepository.GetProductById(productId);
                if (product != null)
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    product.ImageUrl = remaining.Any()
                        ? $"{baseUrl}{Url.Action(nameof(GetFile), new { fileName = remaining.First().FileName })}"
                        : null;
                    await _productRepository.UpdateProduct(product);
                }
            }

            return Ok();
        }
    }
}
