using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for VacationSchedule entity.
/// </summary>
public class VacationScheduleConfiguration : IEntityTypeConfiguration<VacationSchedule>
{
    public void Configure(EntityTypeBuilder<VacationSchedule> builder)
    {
        builder.ToTable("VacationSchedules");
        builder.HasKey(v => v.Id);

        // Properties
        builder.Property(v => v.UserId)
            .IsRequired();

        builder.Property(v => v.SubstituteUserId)
            .IsRequired();

        builder.Property(v => v.StartDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(v => v.EndDate)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(v => v.SourceRequestId)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Substitute)
            .WithMany()
            .HasForeignKey(v => v.SubstituteUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.SourceRequest)
            .WithMany()
            .HasForeignKey(v => v.SourceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for querying
        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.SubstituteUserId);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.StartDate);
        builder.HasIndex(v => v.EndDate);
        builder.HasIndex(v => new { v.StartDate, v.EndDate });
    }
}
