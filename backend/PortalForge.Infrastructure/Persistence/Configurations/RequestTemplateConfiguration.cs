using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class RequestTemplateConfiguration : IEntityTypeConfiguration<RequestTemplate>
{
    public void Configure(EntityTypeBuilder<RequestTemplate> builder)
    {
        builder.ToTable("RequestTemplates", "public");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rt => rt.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(rt => rt.Icon)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.DepartmentId)
            .HasMaxLength(200);

        builder.Property(rt => rt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(rt => rt.RequiresApproval)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(rt => rt.RequiresSubstituteSelection)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(rt => rt.CreatedBy)
            .WithMany()
            .HasForeignKey(rt => rt.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(rt => rt.Fields)
            .WithOne(f => f.RequestTemplate)
            .HasForeignKey(f => f.RequestTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rt => rt.ApprovalStepTemplates)
            .WithOne(ast => ast.RequestTemplate)
            .HasForeignKey(ast => ast.RequestTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rt => rt.QuizQuestions)
            .WithOne(qq => qq.RequestTemplate)
            .HasForeignKey(qq => qq.RequestTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rt => rt.Requests)
            .WithOne(r => r.RequestTemplate)
            .HasForeignKey(r => r.RequestTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(rt => rt.Category);
        builder.HasIndex(rt => rt.DepartmentId);
        builder.HasIndex(rt => rt.IsActive);
    }
}

