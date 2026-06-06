using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CifraShop.Infrastructure.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Product> Products => Set<Product>();

        // Конструктор
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {

            //Database.EnsureCreated();
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{

        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<Admin>(entity =>
        //    {
        //        entity.Property(a => a.Id)
        //              .UseIdentityColumn(seed: 0, increment: 1);
        //    });

        //    // ... остальная конфигурация

        //    var (admins, students, products, orders, orderItems) = DbSeeder.GenerateSeedData();

        //    modelBuilder.Entity<Admin>().HasData(admins);

        //    modelBuilder.Entity<Student>().HasData(students);
        //    modelBuilder.Entity<Product>().HasData(products);
        //    modelBuilder.Entity<Order>().HasData(orders);
        //    modelBuilder.Entity<OrderItem>().HasData(orderItems);
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.ApplyConfiguration(new AdminConfiguration());
            //modelBuilder.ApplyConfiguration(new OrderConfiguration());
            //modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
            //modelBuilder.ApplyConfiguration(new ProductConfiguration());
            //modelBuilder.ApplyConfiguration(new StudentConfiguration());
            modelBuilder.Entity<User>(entity =>
                entity.Property(u => u.Id).UseIdentityColumn(seed: 0, increment: 1));


            modelBuilder.Entity<Product>(entity =>
                entity.Property(p => p.Id).UseIdentityColumn(seed: 0, increment: 1));

            modelBuilder.Entity<Order>(entity =>
                entity.Property(o => o.Id).UseIdentityColumn(seed: 0, increment: 1));

            modelBuilder.Entity<OrderItem>(entity =>
            //    entity.Property(oi => oi.Id).UseIdentityColumn(seed: 0, increment: 1));

            // Здесь же можно настроить связи, если они не определены атрибутами
            // Например:
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.OrderInOrder)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId));

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductInOrder)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
            //var (admins, students, products, orders, orderItems) = DbSeeder.GenerateSeedData();

            //modelBuilder.Entity<Admin>().HasData(admins);

            //modelBuilder.Entity<Student>().HasData(students);
            //modelBuilder.Entity<Product>().HasData(products);
            //modelBuilder.Entity<Order>().HasData(orders);
            //modelBuilder.Entity<OrderItem>().HasData(orderItems);
        }
    }
}

