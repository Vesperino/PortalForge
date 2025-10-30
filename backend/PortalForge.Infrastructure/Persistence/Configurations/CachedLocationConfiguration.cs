using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class CachedLocationConfiguration : IEntityTypeConfiguration<CachedLocation>
{
    public void Configure(EntityTypeBuilder<CachedLocation> builder)
    {
        builder.ToTable("CachedLocations", "public");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Latitude)
            .IsRequired()
            .HasPrecision(10, 7);

        builder.Property(c => c.Longitude)
            .IsRequired()
            .HasPrecision(10, 7);

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

