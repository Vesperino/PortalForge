namespace PortalForge.Domain.Enums;

/// <summary>
/// Hierarchical department roles used for organizational structure and request routing.
/// Higher values indicate higher authority (BoardMember > President > VP > Director > Manager > TeamLead > Employee).
/// </summary>
public enum DepartmentRole
{
    /// <summary>
    /// Regular employee - no supervisory responsibilities.
    /// </summary>
    Employee = 0,

    /// <summary>
    /// Team lead - supervises a small team (2-5 people).
    /// </summary>
    TeamLead = 1,

    /// <summary>
    /// Manager - supervises multiple teams or a department.
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Director - oversees multiple departments.
    /// </summary>
    Director = 3,

    /// <summary>
    /// Vice President - senior leadership position.
    /// </summary>
    VP = 4,

    /// <summary>
    /// President/CEO - top executive.
    /// </summary>
    President = 5,

    /// <summary>
    /// Board Member - highest governance level.
    /// </summary>
    BoardMember = 6
}

