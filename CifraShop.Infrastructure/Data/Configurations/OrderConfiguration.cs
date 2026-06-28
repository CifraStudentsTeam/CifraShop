using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
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
                   .HasColumnName("Sum")
                   .IsRequired();
            builder.Property(o => o.Branch)
                   .HasColumnName("Branch")
                   .HasMaxLength(100)
                   .IsRequired()
                   .HasDefaultValue(string.Empty);
            builder.HasOne(o => o.Customer)
                   .WithMany(u => u.Orders)
                   .HasForeignKey(o => o.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.OrderItems)
                   .WithOne(oi => oi.Order)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
