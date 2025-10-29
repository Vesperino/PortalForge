namespace PortalForge.Domain.Entities;

public class UserRoleGroup
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleGroupId { get; set; }
    public RoleGroup RoleGroup { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; }
    public User? AssignedByUser { get; set; }
}

