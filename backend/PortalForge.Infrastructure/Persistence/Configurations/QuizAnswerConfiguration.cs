using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("QuizAnswers", "public");

        builder.HasKey(qa => qa.Id);

        builder.Property(qa => qa.SelectedAnswer)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(qa => qa.IsCorrect)
            .IsRequired();

        builder.Property(qa => qa.AnsweredAt)
            .IsRequired();

        // Relationships are configured in RequestApprovalStepConfiguration and QuizQuestionConfiguration

        // Indexes
        builder.HasIndex(qa => qa.RequestApprovalStepId);
        builder.HasIndex(qa => qa.QuizQuestionId);
    }
}

