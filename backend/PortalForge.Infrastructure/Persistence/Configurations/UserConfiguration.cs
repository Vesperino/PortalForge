using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "public");

        builder.HasKey(u => u.Id);

        // Authentication fields
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        // Employee personal information (required)
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Employee work information (required)
        builder.Property(u => u.Department)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Position)
            .IsRequired()
            .HasMaxLength(200);

        // Optional fields
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(u => u.ProfilePhotoUrl)
            .HasMaxLength(500);

        // Supervisor relationship (self-referencing)
        builder.HasOne(u => u.Supervisor)
            .WithMany(u => u.Subordinates)
            .HasForeignKey(u => u.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore computed property
        builder.Ignore(u => u.FullName);
    }
}
