using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class ApprovalDelegationConfiguration : IEntityTypeConfiguration<ApprovalDelegation>
{
    public void Configure(EntityTypeBuilder<ApprovalDelegation> builder)
    {
        builder.ToTable("ApprovalDelegations", "public");

        builder.HasKey(ad => ad.Id);

        builder.Property(ad => ad.StartDate)
            .IsRequired();

        builder.Property(ad => ad.EndDate);

        builder.Property(ad => ad.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ad => ad.Reason)
            .HasMaxLength(500);

        builder.Property(ad => ad.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(ad => ad.FromUser)
            .WithMany()
            .HasForeignKey(ad => ad.FromUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ad => ad.ToUser)
            .WithMany()
            .HasForeignKey(ad => ad.ToUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ad => ad.FromUserId);
        builder.HasIndex(ad => ad.ToUserId);
        builder.HasIndex(ad => new { ad.IsActive, ad.StartDate, ad.EndDate });
    }
}