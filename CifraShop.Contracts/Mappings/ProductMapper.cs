using CifraShop.Contracts.Responses.Products;
using CifraShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Contracts.Mappings
{
    public static class ProductMapper
    {
        public static ProductResponce ToResponse(this Product product)
        {
            return new ProductResponce
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Quantity = product.Quantity,
                Status = product.Status,
            };
        }
    }
}
