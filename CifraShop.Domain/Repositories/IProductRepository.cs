using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAll();
        Task<(List<Product> Items, int TotalCount)> GetAllPaged(int page, int pageSize, string? search = null, StatusProduct? status = null, string? branch = null);
        Task<Product?> GetProductById(int id);
        Task<List<Product>> GetProductsByName(string name);
        Task<List<Product>> GetProductsByPrice(int price);
        Task<List<Product>> GetProductsByQuantity(int quantity);
        Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct);
        Task AddProduct(Product productToAdd);
        Task UpdateProduct(Product productToUpdate);
        Task DeleteProduct(Product productToDelete);
        Task DeleteRange(List<int> ids);
        Task UpdateStatusRange(List<int> ids, StatusProduct newStatus);
    }
}
