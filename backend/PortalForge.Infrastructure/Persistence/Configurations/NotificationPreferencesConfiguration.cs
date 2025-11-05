using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.HasKey(np => np.Id);

        builder.Property(np => np.Id)
            .IsRequired();

        builder.Property(np => np.UserId)
            .IsRequired();

        builder.Property(np => np.EmailEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.InAppEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.DigestEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(np => np.DigestFrequency)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(np => np.DisabledTypes)
            .IsRequired()
            .HasDefaultValue("[]")
            .HasMaxLength(1000);

        builder.Property(np => np.GroupSimilarNotifications)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.MaxGroupSize)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(np => np.GroupingTimeWindowMinutes)
            .IsRequired()
            .HasDefaultValue(60);

        builder.Property(np => np.RealTimeEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.CreatedAt)
            .IsRequired();

        builder.Property(np => np.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(np => np.User)
            .WithMany()
            .HasForeignKey(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(np => np.UserId)
            .IsUnique();

        builder.HasIndex(np => new { np.DigestEnabled, np.DigestFrequency });

        builder.ToTable("NotificationPreferences");
    }
}