namespace PortalForge.Application.UseCases.Admin.Queries.GetUsers;

public class GetUsersResult
{
    public List<AdminUserDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Department { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string Position { get; set; } = string.Empty;
    public Guid? PositionId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool MustChangePassword { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> RoleGroups { get; set; } = new();

    // Vacation data
    public int AnnualVacationDays { get; set; }
    public int VacationDaysUsed { get; set; }
    public int OnDemandVacationDaysUsed { get; set; }
    public int CarriedOverVacationDays { get; set; }
}

