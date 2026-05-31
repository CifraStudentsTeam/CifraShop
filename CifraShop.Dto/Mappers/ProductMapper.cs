using CifraShop.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto.Mappers
{
    public static class ProductMapper
    {
        public static ProductResponce ToResponse(Product product)
        {
            return new ProductResponce
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                Status = product.Status.ToString(),
                ThePathToTheImage = product.ThePathToTheImage
            };
        }
    }
}
