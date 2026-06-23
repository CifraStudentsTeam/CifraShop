using CifraShop.Domain.Entities;

namespace CifraShop.Application.Services.Interfaces
{
    public interface IOrderImageService
    {
        Task<List<OrderImage>> GetByOrderId(int orderId);
        Task<OrderImage?> GetById(int id);
        Task<int> GetCountByOrderId(int orderId);
        Task<OrderImage> Add(OrderImage image);
        Task Update(OrderImage image);
        Task Delete(OrderImage image);
    }
}
