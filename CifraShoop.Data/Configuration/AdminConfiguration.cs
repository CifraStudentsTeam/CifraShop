using CifraShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Data.Configuration
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("Admins");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(a => a.Name)
                   .HasMaxLength(30)
                   .IsRequired();
            builder.Property(a => a.SurName)
                   .HasMaxLength(40)
                   .IsRequired();
            builder.Property(a => a.EMail)
                   .HasMaxLength(50)
                   .IsRequired();
            builder.Property(a => a.Password)
                   .HasMaxLength(18)
                   .IsRequired();
        }
    }
}
