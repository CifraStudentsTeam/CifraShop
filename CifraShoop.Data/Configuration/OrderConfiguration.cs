using CifraShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Data.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(o => o.Sum)
                   .IsRequired();
            builder.Property(o => o.DateOfPurchase)
                   .HasDefaultValueSql("GETUTCDATE()");
            builder.Property(o => o.CustomerLogin)
                   .HasMaxLength(18)
                   .IsRequired();
            builder.Property(o => o.Status)
                   .HasConversion<string>()
                   .IsRequired();
            builder.Ignore(o => o.OrderItems);
            builder.HasMany(o => o.OrderItems)
                   .WithOne(oi => oi.Order)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
