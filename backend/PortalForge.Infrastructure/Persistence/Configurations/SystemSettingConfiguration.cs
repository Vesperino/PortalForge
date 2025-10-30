using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings", "public");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // Unique index on Key
        builder.HasIndex(s => s.Key)
            .IsUnique();

        builder.HasOne(s => s.UpdatedByUser)
            .WithMany()
            .HasForeignKey(s => s.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed default values
        var seedDate = new DateTime(2025, 10, 30, 9, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new SystemSetting
            {
                Id = 1,
                Key = "Storage:BasePath",
                Value = "C:\\PortalForge\\Storage",
                Category = "Storage",
                Description = "Base directory path for file storage",
                UpdatedAt = seedDate
            },
            new SystemSetting
            {
                Id = 2,
                Key = "Storage:NewsImagesPath",
                Value = "news-images",
                Category = "Storage",
                Description = "Subdirectory for news images (relative to BasePath)",
                UpdatedAt = seedDate
            },
            new SystemSetting
            {
                Id = 3,
                Key = "Storage:DocumentsPath",
                Value = "documents",
                Category = "Storage",
                Description = "Subdirectory for documents (relative to BasePath)",
                UpdatedAt = seedDate
            },
            new SystemSetting
            {
                Id = 4,
                Key = "Storage:MaxFileSizeMB",
                Value = "10",
                Category = "Storage",
                Description = "Maximum file size in megabytes",
                UpdatedAt = seedDate
            }
        );
    }
}

