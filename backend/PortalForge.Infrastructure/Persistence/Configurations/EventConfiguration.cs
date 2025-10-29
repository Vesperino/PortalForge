using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events", "public");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate);

        builder.Property(e => e.Location)
            .HasMaxLength(500);

        builder.Property(e => e.Tags)
            .HasConversion(
                v => string.Join(',', v.Select(t => t.ToString())),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => Enum.Parse<EventTag>(t))
                    .ToList()
            )
            .HasMaxLength(500);

        builder.Property(e => e.TargetDepartments)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList()
            )
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedBy)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.Attendees)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => e.CreatedAt);
    }
}
