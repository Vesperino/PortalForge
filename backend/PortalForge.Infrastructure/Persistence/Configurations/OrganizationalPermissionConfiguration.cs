using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for OrganizationalPermission entity.
/// </summary>
public class OrganizationalPermissionConfiguration : IEntityTypeConfiguration<OrganizationalPermission>
{
    public void Configure(EntityTypeBuilder<OrganizationalPermission> builder)
    {
        builder.ToTable("OrganizationalPermissions");
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.CanViewAllDepartments)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.VisibleDepartmentIds)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasDefaultValue("[]");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Relationships
        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<OrganizationalPermission>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}
