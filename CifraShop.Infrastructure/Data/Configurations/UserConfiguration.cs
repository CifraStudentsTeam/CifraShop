using CifraShop.Domain.Entities;
using CifraShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                   .HasColumnName("Id")
                   .UseIdentityColumn();
            builder.Property(u => u.Email)
                   .HasColumnName("Email")
                   .HasMaxLength(40)
                   .IsRequired();
            builder.Property(u => u.Password)
                   .HasColumnName("Password")
                   .HasMaxLength(128)
                   .IsRequired();
            builder.Property(u => u.Balance)
                   .HasColumnName("Balance")
                   .IsRequired(false);
            builder.Property(u => u.Role)
                   .HasColumnName("UserRole")
                   .HasConversion<string>()
                   .IsRequired();
            builder.Property(u => u.Branch)
                   .HasColumnName("Branch")
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.HasMany(u => u.Orders)
                   .WithOne(o => o.Customer)
                   .HasForeignKey(o => o.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
