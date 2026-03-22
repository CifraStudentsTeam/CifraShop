using CifraShop.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Components.Services
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Product> Product => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public ApplicationContext() => Database.EnsureCreated();

        public ApplicationContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data source=CifraChop.db");
            }
        }
    }
}
