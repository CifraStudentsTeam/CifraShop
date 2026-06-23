using CifraShop.Application.Services.Interfaces;
using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;

namespace CifraShop.Application.Services.Implementations
{
    public class OrderImageService : IOrderImageService
    {
        private readonly IOrderImageRepository _repository;

        public OrderImageService(IOrderImageRepository repository)
            => _repository = repository;

        public Task<List<OrderImage>> GetByOrderId(int orderId)
            => _repository.GetByOrderId(orderId);

        public Task<OrderImage?> GetById(int id)
            => _repository.GetById(id);

        public Task<int> GetCountByOrderId(int orderId)
            => _repository.GetCountByOrderId(orderId);

        public async Task<OrderImage> Add(OrderImage image)
        {
            await _repository.Add(image);
            return image;
        }

        public Task Update(OrderImage image)
            => _repository.Update(image);

        public Task Delete(OrderImage image)
            => _repository.Delete(image);
    }
}
