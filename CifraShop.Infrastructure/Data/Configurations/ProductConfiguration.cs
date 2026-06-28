using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .HasColumnName("Id")
                   .UseIdentityColumn();
            builder.Property(p => p.Name)
                   .HasColumnName("Name")
                   .HasMaxLength(30)
                   .IsRequired();
            builder.Property(p => p.Description)
                   .HasColumnName("Description")
                   .HasMaxLength(100)
                   .IsRequired();
            builder.Property(p => p.Price)
                   .HasColumnName("Price")
                   .IsRequired();
            builder.Property(p => p.Quantity)
                   .HasColumnName("Quantity")
                   .IsRequired();
            builder.Property(p => p.Status)
                   .HasColumnName("StatusProduct")
                   .HasConversion<string>()
                   .IsRequired();
            builder.Property(p => p.ImageUrl)
                   .HasColumnName("ImageUrl")
                   .HasMaxLength(500)
                   .IsRequired(false);
            builder.Property(p => p.Branch)
                   .HasColumnName("Branch")
                   .HasMaxLength(100)
                   .IsRequired()
                   .HasDefaultValue(string.Empty);

            builder.HasMany(p => p.OrderItems)
                   .WithOne(oi => oi.Product)
                   .HasForeignKey(oi => oi.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
