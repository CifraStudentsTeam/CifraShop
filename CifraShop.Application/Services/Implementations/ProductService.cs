using CifraShop.Application.Services.Interfaces;
using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;

namespace CifraShop.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
            => _repository = repository;

        public Task<List<Product>> GetAllProducts()
            => _repository.GetAll();

        public async Task<PagedResponse<Product>> GetProductsPaged(int page, int pageSize)
        {
            var (items, total) = await _repository.GetAllPaged(page, pageSize);
            return new PagedResponse<Product> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public Task<Product> GetProductById(int id)
            => _repository.GetProductById(id);

        public Task<List<Product>> GetProductsByName(string name)
            => _repository.GetProductsByName(name);

        public Task<List<Product>> GetProductsByPrice(short price)
            => _repository.GetProductsByPrice(price);

        public Task<List<Product>> GetProductsByQuantity(short quantity)
            => _repository.GetProductsByQuantity(quantity);

        public Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct)
            => _repository.GetProductsByStatus(statusProduct);

        public async Task<Product> CreateProduct(string name, string description, short price, short quantity)
        {
            var product = new Product
            {
                Name = name,
                Description = description,
                Price = price,
                Quantity = quantity,
                Status = StatusProduct.OnSaleSoon
            };

            await _repository.AddProduct(product);
            return product;
        }

        public Task UpdateProduct(Product productToUpdate)
            => _repository.UpdateProduct(productToUpdate);

        public Task DeleteProduct(Product productToDelete)
            => _repository.DeleteProduct(productToDelete);

        public Task DeleteRange(List<int> ids)
            => _repository.DeleteRange(ids);
    }
}
