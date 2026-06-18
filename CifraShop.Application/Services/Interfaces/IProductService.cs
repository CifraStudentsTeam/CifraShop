using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProducts();
        Task<Product> GetProductById(int id);
        Task<List<Product>> GetProductsByName(string name);
        Task<List<Product>> GetProductsByPrice(short price);
        Task<List<Product>> GetProductsByQuantity(short quantity);
        Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct);
        Task<Product> CreateProduct(string name, string description, short price, short quantity);
        Task UpdateProduct(Product productToUpdate);
        Task DeleteProduct(Product productToDelete);
    }
}
