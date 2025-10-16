namespace PortalForge.Domain.Entities;

public class User
{
    public Guid Id { get; set; }  // Guid from Supabase Auth
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; } = false;
}

public enum UserRole
{
    Admin,
    Manager,
    HR,
    Marketing,
    Employee
}
