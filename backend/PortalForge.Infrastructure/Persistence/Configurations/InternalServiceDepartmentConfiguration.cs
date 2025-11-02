using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalForge.Domain.Entities;

namespace PortalForge.Infrastructure.Persistence.Configurations;

public class InternalServiceDepartmentConfiguration : IEntityTypeConfiguration<InternalServiceDepartment>
{
    public void Configure(EntityTypeBuilder<InternalServiceDepartment> builder)
    {
        builder.ToTable("InternalServiceDepartments", "public");

        builder.HasKey(sd => new { sd.InternalServiceId, sd.DepartmentId });

        builder.Property(sd => sd.AssignedAt)
            .IsRequired();

        builder.HasOne(sd => sd.InternalService)
            .WithMany(s => s.ServiceDepartments)
            .HasForeignKey(sd => sd.InternalServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sd => sd.Department)
            .WithMany(d => d.DepartmentServices)
            .HasForeignKey(sd => sd.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
