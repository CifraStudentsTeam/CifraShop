using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        //Конструктор
        public ProductService(IProductRepository repository)
            => _repository = repository;

        //Получение всех заказов
        public Task<List<Product>> GetAllProducts()
            => _repository.UploadingProductData();

        //Получение продукта по id
        public Task<Product> GetProductsById(int id)
            => _repository.GetProductsById(id);
        
        //Получение продуктов по имени
        public Task<List<Product>> GetProductsByName(string name)
            => _repository.GetProductsByName(name);

        //Получение продукта по цене
        public Task<List<Product>> GetProductsByPrice(short price)
            => _repository.GetProductsByPrice(price);

        //Получение продуктов по количеству
        public Task<List<Product>> GetProductsByQuntity(short quntity)
            => _repository.GetProductsByQuntity(quntity);

        //Получение продукта по статусу 
        public Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct)
            => _repository.GetProductsByStatus(statusProduct);

        //Создание продукта
        public async Task<Product> CreateProductData(string name, string description, short price, short quantity)
        {
            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Quantity = quantity,
                Status = StatusProduct.OnSaleSoon
            };

            await _repository.AddPoduct(product);
            return product; 
        }

        //Обовление продукта
        public Task UpdateProduct(Product productToUpdate)
            => _repository.UpdateProduct(productToUpdate);

        //Удаление продукта
        public Task DeleteProduct(Product productToDelete)
            => _repository.DeleteProduct(productToDelete);
    }   
}
