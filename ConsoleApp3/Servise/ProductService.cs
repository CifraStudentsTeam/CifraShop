using ConsoleApp3.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3.Servise
{
    public class ProductService : IProductService
    {
        private readonly AplicationContext _context = new AplicationContext();

        public ProductService(AplicationContext context)
           => _context = context;
        public async Task<List<Product>> UploadingProductData()
            => await _context.Product.ToListAsync();

        public async Task<List<Product>> GetProductsByName(string name)
            => await _context.Product.Where(x => x.Name == name).ToListAsync();

        public async Task<List<Product>> GetProductsByPrice(uint price)
            => await _context.Product.Where(x => x.Price == price).ToListAsync();

        public async Task<List<Product>> GetProductsByQuntity(uint quntity)
            => await _context.Product.Where(x => x.Quantity == quntity).ToListAsync();

        public async Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct)
            => await _context.Product.Where(x => x.Status == statusProduct).ToListAsync();

        public async Task<Product> GetProductsById(uint id)
            => await _context.Product.SingleOrDefaultAsync(x => x.Id == id);
        
        public async Task<Product> CreateProduct(string name, string description, uint price, uint quntity, StatusProduct statusProduct)
        {
            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Quantity = quntity,
                Status = statusProduct
            };

            await _context.Product.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task ChangeProductName(Product product, string name)
        {
            product.Name = name;
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
        }

   
        public async Task ChangeProductPrice(Product product, uint price)
        {
            product.Price = price;
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeProductQuntity(Product product, uint quntity)
        {
            product.Quantity = quntity;
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeProductStatus(Product product, StatusProduct statusProduct)
        {
            product.Status = statusProduct;
            _context.Product.Update(product);
            await _context.SaveChangesAsync();
        }

        

        public async Task DeleteProduct(Product product)
        {
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
        }

       

       

        

       

      
        
    }

}
