using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Department entity.
/// Configures self-referencing hierarchy and relationships.
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);

        // Properties
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(2000);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired(false);

        // Self-referencing relationship (Parent-Child)
        builder.HasOne(d => d.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Department Head relationship (Manager)
        builder.HasOne(d => d.HeadOfDepartment)
            .WithMany()
            .HasForeignKey(d => d.HeadOfDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Department Director relationship
        builder.HasOne(d => d.Director)
            .WithMany()
            .HasForeignKey(d => d.DirectorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Head of Department Substitute relationship
        builder.HasOne(d => d.HeadOfDepartmentSubstitute)
            .WithMany()
            .HasForeignKey(d => d.HeadOfDepartmentSubstituteId)
            .OnDelete(DeleteBehavior.SetNull);

        // Director Substitute relationship
        builder.HasOne(d => d.DirectorSubstitute)
            .WithMany()
            .HasForeignKey(d => d.DirectorSubstituteId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        builder.HasIndex(d => d.ParentDepartmentId);
        builder.HasIndex(d => d.HeadOfDepartmentId);
        builder.HasIndex(d => d.DirectorId);
        builder.HasIndex(d => d.HeadOfDepartmentSubstituteId);
        builder.HasIndex(d => d.DirectorSubstituteId);
        builder.HasIndex(d => d.IsActive);
        builder.HasIndex(d => d.Name);
    }
}
