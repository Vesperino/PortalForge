using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Repository interface for managing sick leave (L4) records.
/// </summary>
public interface ISickLeaveRepository
{
    /// <summary>
    /// Gets a sick leave by its ID.
    /// </summary>
    Task<SickLeave?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all sick leave records.
    /// </summary>
    Task<IEnumerable<SickLeave>> GetAllAsync();

    /// <summary>
    /// Creates a new sick leave record.
    /// </summary>
    /// <returns>The ID of the created sick leave.</returns>
    Task<Guid> CreateAsync(SickLeave sickLeave);

    /// <summary>
    /// Updates an existing sick leave record.
    /// </summary>
    Task UpdateAsync(SickLeave sickLeave);

    /// <summary>
    /// Deletes a sick leave record by ID.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets all sick leaves for a specific user.
    /// </summary>
    Task<List<SickLeave>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets all sick leaves within a date range for a user.
    /// </summary>
    Task<List<SickLeave>> GetByUserAndDateRangeAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Gets all active sick leaves (currently ongoing).
    /// </summary>
    Task<List<SickLeave>> GetActiveAsync();

    /// <summary>
    /// Gets sick leaves that require ZUS documentation.
    /// </summary>
    Task<List<SickLeave>> GetRequiringZusDocumentAsync();
}
