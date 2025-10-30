using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests", "public");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequestNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.RequestNumber)
            .IsUnique();

        builder.Property(r => r.SubmittedAt)
            .IsRequired();

        builder.Property(r => r.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Domain.Enums.RequestPriority.Standard);

        builder.Property(r => r.FormData)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Domain.Enums.RequestStatus.Draft);

        // Relationships
        builder.HasOne(r => r.RequestTemplate)
            .WithMany(rt => rt.Requests)
            .HasForeignKey(r => r.RequestTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.SubmittedBy)
            .WithMany()
            .HasForeignKey(r => r.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.ApprovalSteps)
            .WithOne(aps => aps.Request)
            .HasForeignKey(aps => aps.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.SubmittedById);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.SubmittedAt);
    }
}

