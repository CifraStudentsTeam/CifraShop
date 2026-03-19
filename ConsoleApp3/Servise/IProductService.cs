using ConsoleApp3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Servise
{
    public interface IProductService
    {
        public Task<List<Product>> UploadingProductData();
        public Task<Product> CreateProduct(string name, string description, uint price, uint quntity, StatusProduct statusProduct);
        public Task DeleteProduct(Product product);
        public Task<Product> GetProductsById(uint id);
        public Task<List<Product>> GetProductsByName(string name);
        public Task<List<Product>> GetProductsByPrice(uint price);
        public Task<List<Product>> GetProductsByQuntity(uint quntity);
        public Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct);
        public Task ChangeProductName(Product product, string name);
        public Task ChangeProductPrice(Product product, uint price);
        public Task ChangeProductQuntity(Product product, uint quntity);
        public Task ChangeProductStatus(Product product, StatusProduct statusProduct);
    }
}
