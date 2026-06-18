using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Infrastructure.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class OrderRepositoryEfCore : IOrderRepository
    {
        private readonly ApplicationContext _context;

        public OrderRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<Order>> GetAll()
            => await _context.Orders.Include(o => o.OrderItems).ToListAsync();

        public async Task<(List<Order> Items, int TotalCount)> GetAllPaged(int page, int pageSize)
        {
            var query = _context.Orders.Include(o => o.OrderItems);
            var total = await query.CountAsync();
            var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<Order> GetOrderById(int id)
            => await _context.Orders.Include(o => o.OrderItems).SingleOrDefaultAsync(x => x.Id == id);

        public async Task<List<Order>> GetOrdersByLogin(string login)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.CustomerLogin == login).ToListAsync();

        public async Task<List<Order>> GetOrdersByStatus(StatusOrder order)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Status == order).ToListAsync();

        public async Task<List<Order>> GetOrdersBySum(short sum)
            => await _context.Orders.Include(o => o.OrderItems).Where(x => x.Sum == sum).ToListAsync();

        public async Task<List<Order>> GetOrdersByDateRange(DateTime from, DateTime to)
            => await _context.Orders.Include(o => o.OrderItems)
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
            var orders = await _context.Orders.Where(o => ids.Contains(o.Id)).ToListAsync();
            foreach (var order in orders)
                order.Status = newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}
