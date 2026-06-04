
using CifraShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using CifraShop.Data.Additionally;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;

namespace CifraShop.Data.AppDbContext
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Student> Students => Set<Student>();
        
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
            modelBuilder.Entity<Admin>(entity =>
                entity.Property(a => a.Id).UseIdentityColumn(seed: 0, increment: 1));

            modelBuilder.Entity<Student>(entity =>
                entity.Property(s => s.Id).UseIdentityColumn(seed: 0, increment: 1));

            modelBuilder.Entity<Product>(entity =>
                entity.Property(p => p.Id).UseIdentityColumn(seed: 0, increment: 1));

            modelBuilder.Entity<Order>(entity =>
                entity.Property(o => o.Id).UseIdentityColumn(seed: 0, increment: 1));

            modelBuilder.Entity<OrderItem>(entity =>
                entity.Property(oi => oi.Id).UseIdentityColumn(seed: 0, increment: 1));

            // Здесь же можно настроить связи, если они не определены атрибутами
            // Например:
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
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
