using CifraShop.Domain.Entities;
using CifraShop.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CifraShop.Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<AdminAction> AdminActions => Set<AdminAction>();
        public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new OrderConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
            modelBuilder.ApplyConfiguration(new AdminActionConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationSettingsConfiguration());
        }
    }
}
