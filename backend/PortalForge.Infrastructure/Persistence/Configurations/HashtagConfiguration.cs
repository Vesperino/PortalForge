using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class HashtagConfiguration : IEntityTypeConfiguration<Hashtag>
{
    public void Configure(EntityTypeBuilder<Hashtag> builder)
    {
        builder.ToTable("Hashtags", "public");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.NormalizedName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.CreatedAt)
            .IsRequired();

        // Index on NormalizedName for fast searching
        builder.HasIndex(h => h.NormalizedName)
            .IsUnique();

        // Many-to-many relationship with News
        builder.HasMany(h => h.News)
            .WithMany(n => n.Hashtags)
            .UsingEntity<Dictionary<string, object>>(
                "NewsHashtags",
                j => j.HasOne<News>().WithMany().HasForeignKey("NewsId"),
                j => j.HasOne<Hashtag>().WithMany().HasForeignKey("HashtagId"),
                j =>
                {
                    j.ToTable("NewsHashtags", "public");
                    j.HasKey("NewsId", "HashtagId");
                });
    }
}

