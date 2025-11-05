using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.HasKey(nt => nt.Id);

        builder.Property(nt => nt.Id)
            .IsRequired();

        builder.Property(nt => nt.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(nt => nt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(nt => nt.TitleTemplate)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(nt => nt.MessageTemplate)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(nt => nt.EmailSubjectTemplate)
            .HasMaxLength(500);

        builder.Property(nt => nt.EmailBodyTemplate)
            .HasMaxLength(5000);

        builder.Property(nt => nt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(nt => nt.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("pl");

        builder.Property(nt => nt.PlaceholderDefinitions)
            .HasMaxLength(2000);

        builder.Property(nt => nt.CreatedAt)
            .IsRequired();

        builder.Property(nt => nt.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(nt => nt.CreatedBy)
            .WithMany()
            .HasForeignKey(nt => nt.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(nt => new { nt.Type, nt.Language, nt.IsActive });
        builder.HasIndex(nt => nt.IsActive);
        builder.HasIndex(nt => nt.CreatedAt);

        builder.ToTable("NotificationTemplates");
    }
}