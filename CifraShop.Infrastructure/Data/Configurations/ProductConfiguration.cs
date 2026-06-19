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
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.HasMany(p => p.OrderItems)
                   .WithOne(oi => oi.Product)
                   .HasForeignKey(oi => oi.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new Product
                {
                    Id = 1,
                    Name = "Блокнот A5",
                    Description = "Линейный блонкот на 60 листов",
                    Price = 250,
                    Quantity = 50,
                    Status = StatusProduct.InStock,
                    ImageUrl = "notebook_a5.jpg"
                },
                new Product
                {
                    Id = 2,
                    Name = "Ручка шариковая",
                    Description = "Шариковая ручка синего цвета",
                    Price = 35,
                    Quantity = 200,
                    Status = StatusProduct.InStock,
                    ImageUrl = "pen_blue.jpg"
                },
                new Product
                {
                    Id = 3,
                    Name = "Портфель студента",
                    Description = "Вместительный порфель для документов",
                    Price = 1200,
                    Quantity = 15,
                    Status = StatusProduct.InStock,
                    ImageUrl = "portfolio.jpg"
                },
                new Product
                {
                    Id = 4,
                    Name = "USB-флешка 32GB",
                    Description = "USB-флешка для хранения данных",
                    Price = 450,
                    Quantity = 0,
                    Status = StatusProduct.OutOfStock,
                    ImageUrl = "usb_32gb.jpg"
                }
            );
        }
    }
}
