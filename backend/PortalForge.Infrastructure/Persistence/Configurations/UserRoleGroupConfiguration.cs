using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class UserRoleGroupConfiguration : IEntityTypeConfiguration<UserRoleGroup>
{
    public void Configure(EntityTypeBuilder<UserRoleGroup> builder)
    {
        builder.ToTable("UserRoleGroups", "public");

        builder.HasKey(urg => new { urg.UserId, urg.RoleGroupId });

        builder.HasOne(urg => urg.User)
            .WithMany(u => u.UserRoleGroups)
            .HasForeignKey(urg => urg.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(urg => urg.RoleGroup)
            .WithMany(rg => rg.UserRoleGroups)
            .HasForeignKey(urg => urg.RoleGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(urg => urg.AssignedByUser)
            .WithMany()
            .HasForeignKey(urg => urg.AssignedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(urg => urg.AssignedAt)
            .IsRequired();

        builder.HasIndex(urg => urg.UserId);
        builder.HasIndex(urg => urg.RoleGroupId);
    }
}

