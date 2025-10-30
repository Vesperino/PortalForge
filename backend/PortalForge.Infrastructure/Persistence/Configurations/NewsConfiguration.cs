using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.ToTable("News", "public");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.Content)
            .IsRequired();

        builder.Property(n => n.Excerpt)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.ImageUrl)
            .HasMaxLength(1000);

        builder.Property(n => n.AuthorId)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt);

        builder.Property(n => n.Views)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Event-specific fields
        builder.Property(n => n.IsEvent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.EventHashtag)
            .HasMaxLength(100);

        builder.Property(n => n.EventDateTime);

        builder.Property(n => n.EventLocation)
            .HasMaxLength(500);

        builder.Property(n => n.EventPlaceId)
            .HasMaxLength(200);

        builder.Property(n => n.EventLatitude)
            .HasPrecision(10, 7);

        builder.Property(n => n.EventLongitude)
            .HasPrecision(10, 7);

        // Department visibility
        builder.Property(n => n.DepartmentId);

        builder.HasOne(n => n.Author)
            .WithMany()
            .HasForeignKey(n => n.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Event)
            .WithOne(e => e.News)
            .HasForeignKey<News>(n => n.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.Category);
        builder.HasIndex(n => n.IsEvent);
        builder.HasIndex(n => n.DepartmentId);
        builder.HasIndex(n => n.EventDateTime);
    }
}
