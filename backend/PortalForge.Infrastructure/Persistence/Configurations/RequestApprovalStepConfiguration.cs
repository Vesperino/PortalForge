using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RequestApprovalStepConfiguration : IEntityTypeConfiguration<RequestApprovalStep>
{
    public void Configure(EntityTypeBuilder<RequestApprovalStep> builder)
    {
        builder.ToTable("RequestApprovalSteps", "public");

        builder.HasKey(ras => ras.Id);

        builder.Property(ras => ras.StepOrder)
            .IsRequired();

        builder.Property(ras => ras.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Domain.Enums.ApprovalStepStatus.Pending);

        builder.Property(ras => ras.Comment)
            .HasMaxLength(2000);

        builder.Property(ras => ras.RequiresQuiz)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ras => ras.CreatedAt)
            .IsRequired();

        builder.Property(ras => ras.StartedAt);

        builder.Property(ras => ras.FinishedAt);

        builder.Property(ras => ras.EscalatedAt);

        builder.Property(ras => ras.AssignedToUserId);

        builder.Property(ras => ras.StepTemplateId);

        // Relationships
        builder.HasOne(ras => ras.Request)
            .WithMany(r => r.ApprovalSteps)
            .HasForeignKey(ras => ras.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ras => ras.Approver)
            .WithMany()
            .HasForeignKey(ras => ras.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ras => ras.AssignedToUser)
            .WithMany()
            .HasForeignKey(ras => ras.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ras => ras.StepTemplate)
            .WithMany()
            .HasForeignKey(ras => ras.StepTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ras => ras.QuizAnswers)
            .WithOne(qa => qa.RequestApprovalStep)
            .HasForeignKey(qa => qa.RequestApprovalStepId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ras => new { ras.RequestId, ras.StepOrder });
        builder.HasIndex(ras => ras.ApproverId);
        builder.HasIndex(ras => ras.AssignedToUserId);
        builder.HasIndex(ras => ras.Status);
        builder.HasIndex(ras => ras.StepTemplateId);
    }
}

