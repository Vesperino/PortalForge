using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first user with the specified role.
    /// Used by seed handlers to find admin users without loading all records.
    /// </summary>
    Task<User?> GetFirstByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<(IEnumerable<User> Users, int TotalCount)> GetFilteredAsync(
        string? searchTerm,
        string? department,
        string? position,
        string? role,
        bool? isActive,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches users by query string with server-side filtering.
    /// Searches in FirstName, LastName, Email, Department, and Position.
    /// </summary>
    Task<List<User>> SearchAsync(
        string query,
        bool onlyActive,
        Guid? departmentId,
        int limit,
        CancellationToken cancellationToken = default);
}
