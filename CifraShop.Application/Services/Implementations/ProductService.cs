using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IApiService _apiService;
        private const string _baseUri = "api/products";

        public ProductService(IApiService apiService)
            => _apiService = apiService;

        #region Создание продукта
        public async Task<Product> CreateProductAsync(Product product)
            => await _apiService.PostAsync<Product>(_baseUri, product);
        #endregion

        #region Получение данных
        public async Task<List<Product>> GetAllProductsAsync()
            => await _apiService.GetAsync<List<Product>>(_baseUri);

        public async Task<Product> GetProductByIdAsync(int id)
            => await _apiService.GetAsync<Product>($"{_baseUri}/{id}");

        public Task<List<Product>> GetProductsByStatusAsync(StatusProduct status)
            => _apiService.GetAsync<List<Product>>($"{_baseUri}/status/{status}");
        #endregion

        #region Обновление или удаление продукта
        public async Task UpdateProductAsync(Product product)
            => await _apiService.PutAsync($"{_baseUri}/{product.Id}", product);

        public async Task DeleteProductAsync(int id)
            => await _apiService.DeleteAsync($"{_baseUri}/ {id}");
        #endregion
    }
}
