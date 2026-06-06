using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            #region Настройка таблицыи с заказом
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id)
                   .HasColumnName("Id")
                   .UseIdentityColumn();
            builder.Property(o => o.Status)
                   .HasColumnName("StatusOrder")
                   .HasConversion<string>()
                   .IsRequired();
            builder.Property(o => o.Sum)
                   .HasColumnName("DateOfPurchase")
                   .IsRequired();
            builder.Property(o => o.CustomerLogin)
                   .HasColumnName("CustomerLogin")
                   .HasMaxLength(40)
                   .IsRequired();
            #endregion

            #region Настройка связей с заказом
            builder.HasMany(o => o.OrderItems)
                   .WithOne(oi => oi.OrderInOrder)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(o => o.Customer)
                   .WithMany(u => u.Orders)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
