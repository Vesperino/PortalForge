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

        builder.Property(ast => ast.ApproverType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Domain.Enums.ApproverType.Role);

        builder.Property(ast => ast.ApproverRole)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(ast => ast.SpecificUserId);

        builder.Property(ast => ast.ApproverGroupId);

        builder.Property(ast => ast.RequiresQuiz)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ast => ast.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(ast => ast.RequestTemplate)
            .WithMany(rt => rt.ApprovalStepTemplates)
            .HasForeignKey(ast => ast.RequestTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ast => ast.SpecificUser)
            .WithMany()
            .HasForeignKey(ast => ast.SpecificUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ast => ast.ApproverGroup)
            .WithMany()
            .HasForeignKey(ast => ast.ApproverGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(ast => new { ast.RequestTemplateId, ast.StepOrder });
    }
}

