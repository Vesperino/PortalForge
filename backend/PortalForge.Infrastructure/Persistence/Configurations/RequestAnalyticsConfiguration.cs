using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RequestAnalyticsConfiguration : IEntityTypeConfiguration<RequestAnalytics>
{
    public void Configure(EntityTypeBuilder<RequestAnalytics> builder)
    {
        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ra => ra.UserId)
            .IsRequired();

        builder.Property(ra => ra.Year)
            .IsRequired();

        builder.Property(ra => ra.Month)
            .IsRequired();

        builder.Property(ra => ra.TotalRequests)
            .IsRequired();

        builder.Property(ra => ra.ApprovedRequests)
            .IsRequired();

        builder.Property(ra => ra.RejectedRequests)
            .IsRequired();

        builder.Property(ra => ra.PendingRequests)
            .IsRequired();

        builder.Property(ra => ra.AverageProcessingTime)
            .IsRequired();

        builder.Property(ra => ra.LastCalculated)
            .IsRequired();

        // Relationships
        builder.HasOne(ra => ra.User)
            .WithMany()
            .HasForeignKey(ra => ra.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ra => ra.UserId)
            .HasDatabaseName("IX_RequestAnalytics_UserId");

        builder.HasIndex(ra => new { ra.UserId, ra.Year, ra.Month })
            .IsUnique()
            .HasDatabaseName("IX_RequestAnalytics_User_Period");

        builder.HasIndex(ra => new { ra.Year, ra.Month })
            .HasDatabaseName("IX_RequestAnalytics_Period");

        // Table name
        builder.ToTable("RequestAnalytics");
    }
}