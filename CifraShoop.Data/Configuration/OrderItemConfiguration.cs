using CifraShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Data.Configuration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);
            builder.Property(oi => oi.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(oi => oi.Quantity)
                   .IsRequired();
            builder.Property(oi => oi.Price)
                   .IsRequired();
            builder.Ignore(oi => oi.OrderId);
            builder.Ignore(oi => oi.ProductId);
            builder.Ignore(oi => oi.Order);
            builder.Ignore(oi => oi.Product);
            builder.HasOne(oi => oi.Order)
                   .WithMany(o => o.OrderItems)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(oi => oi.Product)
                   .WithMany(p => p.OrderItems)
                   .HasForeignKey(oi => oi.ProductId) 
                   .OnDelete(DeleteBehavior.Restrict);
           
        }
    }
}
