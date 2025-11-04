using PortalForge.Domain.Enums;

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
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = false;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DepartmentRole DepartmentRole { get; set; } = DepartmentRole.Employee;

    // NEW: Link to Department entity
    public Guid? DepartmentId { get; set; }
    public Department? DepartmentEntity { get; set; }

    // Link to Position entity
    public Guid? PositionId { get; set; }
    public Position? PositionEntity { get; set; }

    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    public Guid? SupervisorId { get; set; }
    public User? Supervisor { get; set; }

    public ICollection<User> Subordinates { get; set; } = new List<User>();
    public ICollection<UserRoleGroup> UserRoleGroups { get; set; } = new List<UserRoleGroup>();

    // Vacation allowances - current year
    /// <summary>
    /// Annual vacation days entitlement (default: 26 days per Polish law)
    /// </summary>
    public int? AnnualVacationDays { get; set; } = 26;

    /// <summary>
    /// Number of vacation days used in current year
    /// </summary>
    public int? VacationDaysUsed { get; set; } = 0;

    /// <summary>
    /// Number of on-demand vacation days used (max 4 per Polish law)
    /// </summary>
    public int? OnDemandVacationDaysUsed { get; set; } = 0;

    /// <summary>
    /// Number of circumstantial leave days used (for weddings, funerals, births)
    /// </summary>
    public int? CircumstantialLeaveDaysUsed { get; set; } = 0;

    // Vacation allowances - carried over from previous year
    /// <summary>
    /// Vacation days carried over from previous year (must be used by September 30)
    /// </summary>
    public int? CarriedOverVacationDays { get; set; } = 0;

    /// <summary>
    /// Expiry date for carried over vacation days (September 30)
    /// </summary>
    public DateTime? CarriedOverExpiryDate { get; set; }

    // Employment information
    /// <summary>
    /// Date when employee started working
    /// </summary>
    public DateTime? EmploymentStartDate { get; set; }

    /// <summary>
    /// Whether employee is currently on probation period
    /// </summary>
    public bool IsOnProbation { get; set; } = false;

    /// <summary>
    /// When probation period ends
    /// </summary>
    public DateTime? ProbationEndDate { get; set; }

    // Notification settings
    /// <summary>
    /// Whether user wants to receive email notifications (can be disabled in profile)
    /// </summary>
    public bool EmailNotificationsEnabled { get; set; } = true;

    // Computed properties
    /// <summary>
    /// Full name of the user
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Total available vacation days (current year + carried over - used)
    /// </summary>
    public int TotalAvailableVacationDays =>
        (AnnualVacationDays ?? 26) + (CarriedOverVacationDays ?? 0) - (VacationDaysUsed ?? 0);

    /// <summary>
    /// Number of years working in the company
    /// </summary>
    public int YearsOfService => EmploymentStartDate.HasValue
        ? (DateTime.UtcNow.Year - EmploymentStartDate.Value.Year)
        : 0;
}
