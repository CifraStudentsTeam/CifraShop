using CifraShop.Contracts.Responses.Common;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProducts();
        Task<PagedResponse<Product>> GetProductsPaged(int page, int pageSize, string? search = null, StatusProduct? status = null);
        Task<Product?> GetProductById(int id);
        Task<List<Product>> GetProductsByName(string name);
        Task<List<Product>> GetProductsByPrice(int price);
        Task<List<Product>> GetProductsByQuantity(int quantity);
        Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct);
        Task<Product> CreateProduct(string name, string description, int price, int quantity);
        Task UpdateProduct(Product productToUpdate);
        Task DeleteProduct(Product productToDelete);
        Task DeleteRange(List<int> ids);
        Task UpdateStatusRange(List<int> ids, StatusProduct newStatus);
    }
}
