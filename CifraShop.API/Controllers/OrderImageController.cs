using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CifraShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderImageController : ControllerBase
    {
        private readonly IOrderImageRepository _imageRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        public OrderImageController(
            IOrderImageRepository imageRepository,
            IOrderRepository orderRepository,
            IWebHostEnvironment env)
        {
            _imageRepository = imageRepository;
            _orderRepository = orderRepository;
            _env = env;
        }

        [HttpGet("by-order")]
        public async Task<IActionResult> GetByOrderId([FromQuery] int orderId)
        {
            var images = await _imageRepository.GetByOrderId(orderId);
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
            var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var filePath = Path.Combine(webRoot, "images", "orders", fileName);
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
        public async Task<IActionResult> Upload([FromForm] int orderId, [FromForm] IFormFile file, [FromForm] bool isPrimary = false)
        {
            var order = await _orderRepository.GetOrderById(orderId);
            if (order == null) return NotFound($"Заказ с id {orderId} не найден");

            if (file == null || file.Length == 0)
                return BadRequest("Файл не загружен");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Максимальный размер файла — 5 МБ");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return BadRequest("Допустимые форматы: jpg, jpeg, png, webp, gif");

            var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "images", "orders");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{orderId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            OrderImage image;
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var count = await _imageRepository.GetCountByOrderId(orderId);

                image = new OrderImage
                {
                    OrderId = orderId,
                    FileName = fileName,
                    IsPrimary = isPrimary || count == 0,
                    SortOrder = count
                };

                await _imageRepository.Add(image);
            }
            catch
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
                throw;
            }

            var fullUrl = $"{Request.Scheme}://{Request.Host}{Url.Action(nameof(GetFile), new { fileName })}";
            return Ok(new { image.Id, url = fullUrl, image.FileName, image.IsPrimary, image.SortOrder });
        }

        [HttpPost("set-primary")]
        public async Task<IActionResult> SetPrimary([FromQuery] int imageId)
        {
            var image = await _imageRepository.GetById(imageId);
            if (image == null) return NotFound();

            var allImages = await _imageRepository.GetByOrderId(image.OrderId);
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

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var image = await _imageRepository.GetById(id);
            if (image == null) return NotFound();

            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "orders");
            var filePath = Path.Combine(uploadsDir, image.FileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var wasPrimary = image.IsPrimary;
            var orderId = image.OrderId;

            await _imageRepository.Delete(image);

            if (wasPrimary)
            {
                var remaining = await _imageRepository.GetByOrderId(orderId);
                if (remaining.Any())
                {
                    var newPrimary = remaining.First();
                    newPrimary.IsPrimary = true;
                    await _imageRepository.Update(newPrimary);
                }
            }

            return Ok();
        }
    }
}
