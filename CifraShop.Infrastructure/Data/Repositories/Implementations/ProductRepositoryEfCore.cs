using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class ProductRepositoryEfCore : IProductRepository
    {
        private readonly ApplicationContext _context;

        //Конструктор
        public ProductRepositoryEfCore(ApplicationContext context)
           => _context = context;

        //Создание продукта
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

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        //Получение всех продуктов
        public async Task<List<Product>> UploadingProductData()
            => await _context.Products.ToListAsync();

        //Получение продуктов по имени
        public async Task<List<Product>> GetProductsByName(string name)
            => await _context.Products.Where(x => x.Name == name).ToListAsync();
        
        //Получение продуктов по цене
        public async Task<List<Product>> GetProductsByPrice(uint price)
            => await _context.Products.Where(x => x.Price == price).ToListAsync();

        //Получение продуктов по количеству
        public async Task<List<Product>> GetProductsByQuntity(uint quntity)
            => await _context.Products.Where(x => x.Quantity == quntity).ToListAsync();
        
        //Получение продуктов по статусу
        public async Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct)
            => await _context.Products.Where(x => x.Status == statusProduct).ToListAsync();

        //Получение продукта по id
        public async Task<Product> GetProductsById(int id)
            => await _context.Products.SingleOrDefaultAsync(x => x.Id == id);

        //Изминение имени продукта
        public async Task ChangeProductName(Product product, string name)
        {
            product.Name = name;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        //Изминение цены продукта
        public async Task ChangeProductPrice(Product product, uint price)
        {
            product.Price = price;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        //Изминение количества продукта
        public async Task ChangeProductQuntity(Product product, uint quntity)
        {
            product.Quantity = quntity;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        //Изминение статуса продукта
        public async Task ChangeProductStatus(Product product, StatusProduct statusProduct)
        {
            product.Status = statusProduct;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        
        //Удаление продукта
        public async Task DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
