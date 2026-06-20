using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class AdminActionConfiguration : IEntityTypeConfiguration<AdminAction>
    {
        public void Configure(EntityTypeBuilder<AdminAction> builder)
        {
            builder.ToTable("AdminActions");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id)
                   .HasColumnName("Id")
                   .UseIdentityColumn();
            builder.Property(a => a.ActionType)
                   .HasColumnName("ActionType")
                   .HasMaxLength(50)
                   .IsRequired();
            builder.Property(a => a.Details)
                   .HasColumnName("Details")
                   .HasMaxLength(200)
                   .IsRequired();
            builder.Property(a => a.CreatedAt)
                   .HasColumnName("CreatedAt")
                   .IsRequired();
            builder.Property(a => a.Branch)
                   .HasColumnName("Branch")
                   .HasMaxLength(50)
                   .IsRequired()
                   .HasDefaultValue(string.Empty);
        }
    }
}
