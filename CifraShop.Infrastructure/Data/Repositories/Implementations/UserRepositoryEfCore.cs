using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using CifraShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data.Repositories.Implementations
{
    public class UserRepositoryEfCore : IUserRepository
    {
        private readonly ApplicationContext _context;

        public UserRepositoryEfCore(ApplicationContext context)
            => _context = context;

        public async Task<List<User>> GetAllUsers()
            => await _context.Users.ToListAsync();

        public async Task<(List<User> Items, int TotalCount)> GetAllUsersPaged(int page, int pageSize, string? search = null, UserRole? role = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Email.Contains(search));

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            var total = await query.CountAsync();
            var items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<List<User>> GetAllAdmins()
            => await _context.Users.Where(x => x.Role == UserRole.Admin).ToListAsync();

        public async Task<List<User>> GetAllStudents()
            => await _context.Users.Where(x => x.Role == UserRole.Student).ToListAsync();

        public async Task<User?> GetUserById(int id)
            => await _context.Users.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<User?> GetUserByEmail(string email)
            => await _context.Users.SingleOrDefaultAsync(x => x.Email == email);

        public async Task AddUser(User userToAdd)
        {
            await _context.Users.AddAsync(userToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(User userToUpdate)
        {
            _context.Users.Update(userToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(User userToDelete)
        {
            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
