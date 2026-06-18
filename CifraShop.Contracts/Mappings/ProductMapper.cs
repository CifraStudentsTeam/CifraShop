using CifraShop.Contracts.Responses.Products;
using CifraShop.Domain.Entities;

namespace CifraShop.Contracts.Mappings
{
    public static class ProductMapper
    {
        public static ProductResponse ToResponse(this Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                Status = product.Status,
            };
        }
    }
}
