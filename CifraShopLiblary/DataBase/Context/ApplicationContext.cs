using CifraShopLiblary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;

namespace CifraShopLiblary.DataBase.Context
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
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ... остальная конфигурация

            var (admins, students, products, orders, orderItems) = DbSeeder.GenerateSeedData();

            modelBuilder.Entity<Admin>().HasData(admins);
            modelBuilder.Entity<Student>().HasData(students);
            modelBuilder.Entity<Product>().HasData(products);
            modelBuilder.Entity<Order>().HasData(orders);
            modelBuilder.Entity<OrderItem>().HasData(orderItems);
        }
    }
}
