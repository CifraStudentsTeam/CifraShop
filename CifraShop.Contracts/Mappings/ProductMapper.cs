using CifraShop.Contracts.Responses.Products;
using CifraShop.Domain.Entities;

namespace CifraShop.Contracts.Mappings
{
    public static class ProductMapper
    {
        public static ProductResponse ToResponse(this Product product, string? baseUrl = null)
        {
            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                Status = product.Status,
                ImageUrl = product.ImageUrl,
                Branch = product.Branch
            };

            if (product.Images != null && product.Images.Any())
            {
                response.Images = product.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageResponse
                    {
                        Id = i.Id,
                        FileName = i.FileName,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder,
                        Url = baseUrl != null
                            ? $"{baseUrl}/api/ProductImage/file/{i.FileName}"
                            : $"/api/ProductImage/file/{i.FileName}"
                    })
                    .ToList();
            }

            return response;
        }
    }
}
