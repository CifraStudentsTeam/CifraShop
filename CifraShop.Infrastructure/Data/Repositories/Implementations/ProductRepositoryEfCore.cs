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

        
        //Добавление продукта
        public async Task AddPoduct(Product productToAdd)
        {
            await _context.Products.AddAsync(productToAdd);
            await _context.SaveChangesAsync();
        }

        //Обновление продукта
        public async Task UpdateProduct(Product productToUpdate)
        {
            _context.Products.Update(productToUpdate);
            await _context.SaveChangesAsync();
        }

        //Удаление продукта
        public async Task DeleteProduct(Product productToDelete)
        {
            _context.Products.Remove(productToDelete);
            await _context.SaveChangesAsync();
        }

    }
}
