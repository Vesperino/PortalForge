using MediatR;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Users.Queries.SearchUsers;

/// <summary>
/// Query to search users by name, email, department, or position.
/// Used for autocomplete dropdowns and user selection.
/// </summary>
public class SearchUsersQuery : IRequest<List<UserSearchDto>>
{
    /// <summary>
    /// Search query - searches in FirstName, LastName, Email, Department, and Position
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Optional department filter
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Maximum number of results to return (default 10)
    /// </summary>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Only return active users
    /// </summary>
    public bool OnlyActive { get; set; } = true;
}
