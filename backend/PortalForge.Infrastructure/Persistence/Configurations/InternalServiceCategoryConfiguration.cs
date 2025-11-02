using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class InternalServiceCategoryConfiguration : IEntityTypeConfiguration<InternalServiceCategory>
{
    public void Configure(EntityTypeBuilder<InternalServiceCategory> builder)
    {
        builder.ToTable("InternalServiceCategories", "public");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.Icon)
            .HasMaxLength(255);

        builder.Property(c => c.DisplayOrder)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.HasMany(c => c.Services)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
