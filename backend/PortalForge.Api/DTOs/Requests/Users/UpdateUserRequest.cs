namespace PortalForge.Api.DTOs.Requests.Users;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string Position { get; set; } = string.Empty;
    public Guid? PositionId { get; set; }
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<Guid> RoleGroupIds { get; set; } = new();
    public bool IsActive { get; set; }
    public string? NewPassword { get; set; }
}
