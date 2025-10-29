using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RoleGroupPermissionConfiguration : IEntityTypeConfiguration<RoleGroupPermission>
{
    public void Configure(EntityTypeBuilder<RoleGroupPermission> builder)
    {
        builder.ToTable("RoleGroupPermissions", "public");

        builder.HasKey(rgp => new { rgp.RoleGroupId, rgp.PermissionId });

        builder.HasOne(rgp => rgp.RoleGroup)
            .WithMany(rg => rg.RoleGroupPermissions)
            .HasForeignKey(rgp => rgp.RoleGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rgp => rgp.Permission)
            .WithMany(p => p.RoleGroupPermissions)
            .HasForeignKey(rgp => rgp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

