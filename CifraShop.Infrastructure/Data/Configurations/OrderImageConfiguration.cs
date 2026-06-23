using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class OrderImageConfiguration : IEntityTypeConfiguration<OrderImage>
    {
        public void Configure(EntityTypeBuilder<OrderImage> builder)
        {
            builder.ToTable("OrderImages");
            builder.HasKey(oi => oi.Id);
            builder.Property(oi => oi.Id)
                   .HasColumnName("Id")
                   .UseIdentityColumn();
            builder.Property(oi => oi.FileName)
                   .HasColumnName("FileName")
                   .HasMaxLength(200)
                   .IsRequired();
            builder.Property(oi => oi.IsPrimary)
                   .HasColumnName("IsPrimary")
                   .IsRequired();
            builder.Property(oi => oi.SortOrder)
                   .HasColumnName("SortOrder")
                   .IsRequired();

            builder.HasOne(oi => oi.Order)
                   .WithMany(o => o.Images)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
