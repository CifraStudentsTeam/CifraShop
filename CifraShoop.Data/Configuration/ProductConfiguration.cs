using CifraShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Data.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(p => p.Name)
                   .HasMaxLength(30)
                   .IsRequired();
            builder.Property(p => p.Description)
                   .HasMaxLength(1000)
                   .IsRequired();
            builder.Property(p => p.Price)
                   .IsRequired();
            builder.Property(p => p.Quantity)
                   .IsRequired();
            builder.Property(p => p.ThePathToTheImage)
                   .HasMaxLength(500)
                   .IsRequired(false);
            builder.Property(p => p.Status)
                   .HasConversion<string>()
                   .IsRequired();
            builder.Ignore(p => p.IsSelected);
            builder.Ignore(p => p.OrderItems);
            builder.HasMany(p => p.OrderItems)
                   .WithOne(oi => oi.Product)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
