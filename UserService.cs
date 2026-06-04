using CifraShop.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Components.Services
{
    public class UserService: IUserService
    {
        private readonly ApplicationContext _context;

        public UserService(ApplicationContext context)
            => _context = context;

        #region Создание

        public async Task<User> CreateStudent(string name, string surName, string email, string password)
        {
            var user = new User
            {
                Name = name,
                SurName = surName,
                Email = email,
                Password = password,
                Type = TypeUser.Student,
                Balance = 0
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> CreateAdmin(string name, string surName, string email, string password)
        {
            var user = new User
            {
                Name = name,
                SurName = surName,
                Email = email,
                Password = password,
                Type = TypeUser.Admin
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        #endregion

        #region Чтенние данных

        public async Task<List<User>> GetAllUsers()
            => await _context.Users.ToListAsync();

        public async Task<List<User>> GetAllAdmins()
            => await _context.Users.Where(x=> x.Type  == TypeUser.Admin).ToListAsync();

        public async Task<List<User>> GetAllStudents()
            => await _context.Users.Where(x => x.Type == TypeUser.Student).ToListAsync();

        public async Task<User> GetUserById(uint id)
            => await _context.Users.SingleOrDefaultAsync(x => x.Id == id);

        public async Task<User> GetUserByEmail(string email)
            => await _context.Users.SingleOrDefaultAsync(x => x.Email == email);

        public async Task<User> GetUserByEmailAndPassword(string email, string password)
            => await _context.Users.SingleOrDefaultAsync(x => x.Email == email && x.Password == password);

        public async Task<List<User>> GetUsersByBalance(uint balance)
            => await _context.Users.Where(x => x.Balance == balance).ToListAsync();
        #endregion

        #region Удалиние или обновление
        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
