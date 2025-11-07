namespace PortalForge.Api.DTOs.Requests.Users;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "Employee";
    public List<Guid> RoleGroupIds { get; set; } = new();
    public bool MustChangePassword { get; set; } = true;
}
