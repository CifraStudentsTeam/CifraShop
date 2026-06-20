using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            #region Настройка таблицы с состовляющей заказа
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);
            builder.Property(oi => oi.Id)
                   .HasColumnName("Id")
                   .UseIdentityColumn();
            builder.Property(oi => oi.Quantity)
                   .HasColumnName("Quantity")
                   .IsRequired();
            builder.Property(oi => oi.Price)
                   .HasColumnName("Price")
                   .IsRequired();
            #endregion

            #region Настройка связей с состовляющей заказа
            builder.HasOne(oi => oi.OrderInOrder)
                   .WithMany(o => o.OrderItems)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(oi => oi.ProductInOrder)
                   .WithMany(p => p.OrderItems)
                   .HasForeignKey(oi => oi.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
