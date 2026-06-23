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
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Include(o => o.Images).ToListAsync();

        public async Task<(List<Order> Items, int TotalCount)> GetAllPaged(int page, int pageSize, string? search = null, StatusOrder? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Include(o => o.Images).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.Customer.Email.Contains(search) || o.Id.ToString().Contains(search));

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            if (dateFrom.HasValue)
                query = query.Where(o => o.DateOfPurchase >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(o => o.DateOfPurchase <= dateTo.Value.AddDays(1));

            var total = await query.CountAsync();
            var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<Order?> GetOrderById(int id)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Include(o => o.Images).SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Order>> GetOrdersByCustomerEmail(string email)
            => await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Include(o => o.Images)
                .Where(x => x.Customer.Email == email)
                .ToListAsync();

        public async Task<List<Order>> GetOrdersByStatus(StatusOrder order)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Include(o => o.Images).Where(x => x.Status == order).ToListAsync();

        public async Task<List<Order>> GetOrdersBySum(int sum)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Include(o => o.Images).Where(x => x.Sum == sum).ToListAsync();

        public async Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to)
            => await _context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).Include(o => o.Images)
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
            List<(int ProductId, int Quantity)> stockUpdates)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    item.OrderId = order.Id;
                    await _context.OrderItems.AddAsync(item);
                }

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
