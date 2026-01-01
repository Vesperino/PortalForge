namespace PortalForge.Api.DTOs.Requests.Users;

public sealed class CreateUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string Role { get; init; } = "Employee";
    public List<Guid> RoleGroupIds { get; init; } = new();
    public bool MustChangePassword { get; init; } = true;
}
