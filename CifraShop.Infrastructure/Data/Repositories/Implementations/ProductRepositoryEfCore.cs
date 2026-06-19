using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class ProductRepositoryEfCore : IProductRepository
    {
        private readonly ApplicationContext _context;

        public ProductRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<Product>> GetAll()
            => await _context.Products.ToListAsync();

        public async Task<(List<Product> Items, int TotalCount)> GetAllPaged(int page, int pageSize)
        {
            var query = _context.Products;
            var total = await query.CountAsync();
            var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<List<Product>> GetProductsByName(string name)
            => await _context.Products.Where(x => x.Name == name).ToListAsync();

        public async Task<List<Product>> GetProductsByPrice(short price)
            => await _context.Products.Where(x => x.Price == price).ToListAsync();

        public async Task<List<Product>> GetProductsByQuantity(short quantity)
            => await _context.Products.Where(x => x.Quantity == quantity).ToListAsync();

        public async Task<List<Product>> GetProductsByStatus(StatusProduct statusProduct)
            => await _context.Products.Where(x => x.Status == statusProduct).ToListAsync();

        public async Task<Product?> GetProductById(int id)
            => await _context.Products.SingleOrDefaultAsync(x => x.Id == id);

        public async Task AddProduct(Product productToAdd)
        {
            await _context.Products.AddAsync(productToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product productToUpdate)
        {
            _context.Products.Update(productToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(Product productToDelete)
        {
            _context.Products.Remove(productToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRange(List<int> ids)
        {
            var products = _context.Products.Where(p => ids.Contains(p.Id));
            await products.ExecuteDeleteAsync();
        }
    }
}
