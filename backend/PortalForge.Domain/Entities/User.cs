namespace PortalForge.Domain.Entities;

public class User
{
    // Authentication data
    public Guid Id { get; set; }  // Guid from Supabase Auth
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsEmailVerified { get; set; } = false;

    // Employee personal information (required fields from PRD)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Employee work information (required fields from PRD)
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;

    // Optional fields from PRD
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // Supervisor relationship (required from PRD)
    public Guid? SupervisorId { get; set; }
    public User? Supervisor { get; set; }

    // Navigation property for subordinates
    public ICollection<User> Subordinates { get; set; } = new List<User>();

    // Full name computed property
    public string FullName => $"{FirstName} {LastName}";
}
