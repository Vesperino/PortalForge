namespace PortalForge.Api.DTOs.Requests.Users;

public sealed class UpdateUserRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public Guid? DepartmentId { get; init; }
    public string Position { get; init; } = string.Empty;
    public Guid? PositionId { get; init; }
    public string? PhoneNumber { get; init; }
    public string Role { get; init; } = string.Empty;
    public List<Guid> RoleGroupIds { get; init; } = new();
    public bool IsActive { get; init; }
    public string? NewPassword { get; init; }
}
