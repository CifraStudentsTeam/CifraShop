using CifraShop.Domain.Entities;
using CifraShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class ProductImageRepositoryEfCore : IProductImageRepository
    {
        private readonly ApplicationContext _context;

        public ProductImageRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<ProductImage>> GetByProductId(int productId)
            => await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();

        public async Task<ProductImage?> GetById(int id)
            => await _context.ProductImages.FindAsync(id);

        public async Task Add(ProductImage image)
        {
            await _context.ProductImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ProductImage image)
        {
            _context.ProductImages.Update(image);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(ProductImage image)
        {
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCountByProductId(int productId)
            => await _context.ProductImages.CountAsync(i => i.ProductId == productId);
    }
}
