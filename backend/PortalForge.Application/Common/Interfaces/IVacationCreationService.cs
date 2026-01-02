using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Service responsible for creating vacation schedules from approved requests.
/// Handles extraction of vacation details from form data and validation.
/// </summary>
public interface IVacationCreationService
{
    /// <summary>
    /// Creates vacation schedule from approved vacation request.
    /// Extracts start date, end date, and substitute from form data.
    /// Validates substitute is not the user and is an active employee.
    /// </summary>
    /// <param name="vacationRequest">The approved vacation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">If form data is invalid.</exception>
    /// <exception cref="Exceptions.ValidationException">If substitute is invalid (same as user, inactive, etc.).</exception>
    Task CreateFromApprovedRequestAsync(Request vacationRequest, CancellationToken cancellationToken = default);
}
