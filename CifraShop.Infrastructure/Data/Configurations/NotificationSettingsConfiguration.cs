using CifraShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CifraShop.Infrastructure.Data.Configurations
{
    public class NotificationSettingsConfiguration : IEntityTypeConfiguration<NotificationSettings>
    {
        public void Configure(EntityTypeBuilder<NotificationSettings> builder)
        {
            builder.ToTable("NotificationSettings");
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Id)
                   .HasColumnName("Id")
                   .UseIdentityColumn();
            builder.Property(n => n.Email)
                   .HasColumnName("Email")
                   .HasMaxLength(100)
                   .IsRequired();
            builder.Property(n => n.TelegramBotToken)
                   .HasColumnName("TelegramBotToken")
                   .HasMaxLength(100)
                   .IsRequired();
            builder.Property(n => n.TelegramChatId)
                   .HasColumnName("TelegramChatId")
                   .HasMaxLength(50)
                   .IsRequired();
            builder.Property(n => n.Branch)
                   .HasColumnName("Branch")
                   .HasMaxLength(50)
                   .IsRequired();
            builder.Property(n => n.NotifyOnNewOrder)
                   .HasColumnName("NotifyOnNewOrder")
                   .IsRequired();
            builder.Property(n => n.NotifyOnStatusChange)
                   .HasColumnName("NotifyOnStatusChange")
                   .IsRequired();
            builder.Property(n => n.NotifyOnLowStock)
                   .HasColumnName("NotifyOnLowStock")
                   .IsRequired();
            builder.Property(n => n.LowStockThreshold)
                   .HasColumnName("LowStockThreshold")
                   .IsRequired();
        }
    }
}
