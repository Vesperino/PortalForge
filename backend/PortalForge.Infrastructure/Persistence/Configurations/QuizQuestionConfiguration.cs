using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestions", "public");

        builder.HasKey(qq => qq.Id);

        builder.Property(qq => qq.Question)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(qq => qq.Options)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(qq => qq.Order)
            .IsRequired();

        // Relationships are configured in RequestTemplateConfiguration
        
        builder.HasMany(qq => qq.QuizAnswers)
            .WithOne(qa => qa.QuizQuestion)
            .HasForeignKey(qa => qa.QuizQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(qq => new { qq.RequestTemplateId, qq.Order });
    }
}

