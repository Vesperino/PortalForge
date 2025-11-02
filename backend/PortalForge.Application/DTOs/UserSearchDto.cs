namespace PortalForge.Application.DTOs;

/// <summary>
/// Lightweight DTO for user search results in autocomplete components.
/// </summary>
public class UserSearchDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Position { get; set; }
    public string? Department { get; set; }
    public string? DepartmentId { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}
