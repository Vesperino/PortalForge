using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RoleGroupConfiguration : IEntityTypeConfiguration<RoleGroup>
{
    public void Configure(EntityTypeBuilder<RoleGroup> builder)
    {
        builder.ToTable("RoleGroups", "public");

        builder.HasKey(rg => rg.Id);

        builder.Property(rg => rg.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(rg => rg.Name)
            .IsUnique();

        builder.Property(rg => rg.Description)
            .HasMaxLength(500);

        builder.Property(rg => rg.IsSystemRole)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rg => rg.CreatedAt)
            .IsRequired();

        builder.Property(rg => rg.UpdatedAt);
    }
}

