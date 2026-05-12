using CifraShopLiblary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
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
    }
}
