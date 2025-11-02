using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class InternalServiceConfiguration : IEntityTypeConfiguration<InternalService>
{
    public void Configure(EntityTypeBuilder<InternalService> builder)
    {
        builder.ToTable("InternalServices", "public");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.Url)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Icon)
            .HasMaxLength(500);

        builder.Property(s => s.IconType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.DisplayOrder)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.Property(s => s.IsGlobal)
            .IsRequired();

        builder.Property(s => s.IsPinned)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.ServiceDepartments)
            .WithOne(sd => sd.InternalService)
            .HasForeignKey(sd => sd.InternalServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
