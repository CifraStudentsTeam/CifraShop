using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Product");
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
                   .IsRequired();
            builder.Property(p => p.ThePathToTheImage)
                   .HasColumnName("ThePathToTheImage")
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.Ignore(p => p.IsSelected);

            builder.HasMany(p => p.OrderItems)
                   .WithOne(oi => oi.Product)
                   .HasForeignKey(oi => oi.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
