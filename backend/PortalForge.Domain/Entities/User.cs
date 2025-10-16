namespace PortalForge.Domain.Entities;

public class User
{
    // Authentication data
    public Guid Id { get; set; }  
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; } = false;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    public Guid? SupervisorId { get; set; }
    public User? Supervisor { get; set; }

    public ICollection<User> Subordinates { get; set; } = new List<User>();

    // Full name computed property
    public string FullName => $"{FirstName} {LastName}";
}
