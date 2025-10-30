using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RequestTemplateFieldConfiguration : IEntityTypeConfiguration<RequestTemplateField>
{
    public void Configure(EntityTypeBuilder<RequestTemplateField> builder)
    {
        builder.ToTable("RequestTemplateFields", "public");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.FieldType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(f => f.Placeholder)
            .HasMaxLength(500);

        builder.Property(f => f.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.Options)
            .HasColumnType("jsonb");

        builder.Property(f => f.HelpText)
            .HasMaxLength(1000);

        builder.Property(f => f.Order)
            .IsRequired();

        // Relationships are configured in RequestTemplateConfiguration
        
        // Indexes
        builder.HasIndex(f => new { f.RequestTemplateId, f.Order });
    }
}

