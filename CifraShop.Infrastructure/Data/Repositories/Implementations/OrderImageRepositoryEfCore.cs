using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class OrderImageRepositoryEfCore : IOrderImageRepository
    {
        private readonly ApplicationContext _context;

        public OrderImageRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<OrderImage>> GetByOrderId(int orderId)
            => await _context.OrderImages
                .Where(oi => oi.OrderId == orderId)
                .OrderBy(oi => oi.SortOrder)
                .ToListAsync();

        public async Task<OrderImage?> GetById(int id)
            => await _context.OrderImages.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<int> GetCountByOrderId(int orderId)
            => await _context.OrderImages.CountAsync(oi => oi.OrderId == orderId);

        public async Task Add(OrderImage image)
        {
            await _context.OrderImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task Update(OrderImage image)
        {
            _context.OrderImages.Update(image);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(OrderImage image)
        {
            _context.OrderImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }
}
