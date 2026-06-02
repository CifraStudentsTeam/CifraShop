using CifraShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Data.Configuration
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(s => s.LoginName)
                   .HasMaxLength(18)
                   .IsRequired();
            builder.Property(s => s.Password)
                   .HasMaxLength(18)
                   .IsRequired();
            builder.Property(s => s.DateOfBirth)
                   .IsRequired();
            builder.Property(s => s.Balance)
                   .IsRequired();
        }
    }
}
