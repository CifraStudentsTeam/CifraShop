using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Interfaces
{
    public interface IProductRepository
    {
        public Task<List<Product>> UploadingProductData();
        public Task<Product> GetProductsById(int id);
        public Task<List<Product>> GetProductsByName(string name);
        public Task<List<Product>> GetProductsByPrice(short price);
        public Task<List<Product>> GetProductsByQuntity(short quntity);
        public Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct);
        public Task AddPoduct(Product productToAdd);
        public Task UpdateProduct(Product productToUpdate);
        public Task DeleteProduct(Product productToDelete); 
    }
}
