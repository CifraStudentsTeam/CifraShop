using CifraShop.Domain.Entities;

namespace CifraShop.Domain.Repositories
{
    public interface IProductImageRepository
    {
        Task<List<ProductImage>> GetByProductId(int productId);
        Task<ProductImage?> GetById(int id);
        Task Add(ProductImage image);
        Task Update(ProductImage image);
        Task Delete(ProductImage image);
        Task<int> GetCountByProductId(int productId);
    }
}
