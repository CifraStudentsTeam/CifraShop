using CifraShop.Domain.Entities;

namespace CifraShop.Domain.Repositories
{
    public interface IOrderImageRepository
    {
        Task<List<OrderImage>> GetByOrderId(int orderId);
        Task<OrderImage?> GetById(int id);
        Task<int> GetCountByOrderId(int orderId);
        Task Add(OrderImage image);
        Task Update(OrderImage image);
        Task Delete(OrderImage image);
    }
}
