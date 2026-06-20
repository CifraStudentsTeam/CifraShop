using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;

namespace CifraShop.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
            => _repository = repository;

        public Task<List<Product>> GetAllProducts()
            => _repository.GetAll();

        public async Task<PagedResponse<Product>> GetProductsPaged(int page, int pageSize, string? search = null, StatusProduct? status = null)
        {
            if (page < 0)
                throw new ArgumentException("Номер страницы не может быть отрицательным");
            if (pageSize <= 0)
                throw new ArgumentException("Размер страницы должен быть больше 0");

            var (items, total) = await _repository.GetAllPaged(page, pageSize, search, status);
            return new PagedResponse<Product> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public Task<Product?> GetProductById(int id)
            => _repository.GetProductById(id);

        public Task<List<Product>> GetProductsByName(string name)
            => _repository.GetProductsByName(name);

        public Task<List<Product>> GetProductsByPrice(int price)
            => _repository.GetProductsByPrice(price);

        public Task<List<Product>> GetProductsByQuantity(int quantity)
            => _repository.GetProductsByQuantity(quantity);

        public Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct)
            => _repository.GetProductsByStatus(statusProduct);

        public async Task<Product> CreateProduct(string name, string description, int price, int quantity)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название товара обязательно");
            if (price <= 0)
                throw new ArgumentException("Цена товара должна быть больше 0");
            if (quantity < 0)
                throw new ArgumentException("Количество товара не может быть отрицательным");

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Quantity = quantity,
                Status = quantity > 0 ? StatusProduct.InStock : StatusProduct.OutOfStock
            };

            await _repository.AddProduct(product);
            return product;
        }

        public async Task UpdateProduct(Product productToUpdate)
        {
            if (productToUpdate == null)
                throw new ArgumentNullException(nameof(productToUpdate));
            if (string.IsNullOrWhiteSpace(productToUpdate.Name))
                throw new ArgumentException("Название товара обязательно");
            if (productToUpdate.Quantity < 0)
                throw new ArgumentException("Количество товара не может быть отрицательным");

            await _repository.UpdateProduct(productToUpdate);
        }

        public Task DeleteProduct(Product productToDelete)
            => _repository.DeleteProduct(productToDelete);

        public Task DeleteRange(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                throw new ArgumentException("Список id не может быть пустым");
            return _repository.DeleteRange(ids);
        }
    }
}
