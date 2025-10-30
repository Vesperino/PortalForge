using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RequestApprovalStepTemplateConfiguration : IEntityTypeConfiguration<RequestApprovalStepTemplate>
{
    public void Configure(EntityTypeBuilder<RequestApprovalStepTemplate> builder)
    {
        builder.ToTable("RequestApprovalStepTemplates", "public");

        builder.HasKey(ast => ast.Id);

        builder.Property(ast => ast.StepOrder)
            .IsRequired();

        builder.Property(ast => ast.ApproverRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ast => ast.RequiresQuiz)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ast => ast.CreatedAt)
            .IsRequired();

        // Relationships are configured in RequestTemplateConfiguration
        
        // Indexes
        builder.HasIndex(ast => new { ast.RequestTemplateId, ast.StepOrder });
    }
}

