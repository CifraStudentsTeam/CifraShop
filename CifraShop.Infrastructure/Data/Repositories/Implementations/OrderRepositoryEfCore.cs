using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class OrderRepositoryEfCore : IOrderRepository
    {
        private readonly ApplicationContext _context;

        public OrderRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<Order>> GetAll()
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).ToListAsync();

        public async Task<(List<Order> Items, int TotalCount)> GetAllPaged(int page, int pageSize)
        {
            var query = _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer);
            var total = await query.CountAsync();
            var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<Order?> GetOrderById(int id)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Order>> GetOrdersByCustomerEmail(string email)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Where(x => x.Customer.Email == email)
                .ToListAsync();

        public async Task<List<Order>> GetOrdersByStatus(StatusOrder order)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Where(x => x.Status == order).ToListAsync();

        public async Task<List<Order>> GetOrdersBySum(short sum)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Where(x => x.Sum == sum).ToListAsync();

        public async Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer)
                .Where(x => x.DateOfPurchase >= from && x.DateOfPurchase <= to).ToListAsync();

        public async Task AddOrder(Order orderToAdd)
        {
            await _context.Orders.AddAsync(orderToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order orderToUpdate)
        {
            _context.Orders.Update(orderToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusRange(List<int> ids, StatusOrder newStatus)
        {
            await _context.Orders
                .Where(o => ids.Contains(o.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, newStatus));
        }

        public async Task DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> CreateOrderInTransaction(
            Order order,
            List<OrderItem> items,
            List<(int ProductId, short Quantity)> stockUpdates)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                    await _context.OrderItems.AddAsync(item);

                foreach (var (productId, quantity) in stockUpdates)
                {
                    var product = await _context.Products.SingleAsync(p => p.Id == productId);
                    product.Quantity -= quantity;
                    product.Status = product.Quantity == 0
                        ? StatusProduct.OutOfStock
                        : StatusProduct.InStock;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
