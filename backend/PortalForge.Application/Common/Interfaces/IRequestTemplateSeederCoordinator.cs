namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Coordinates all request template seeders and provides a unified interface for seeding operations.
/// </summary>
public interface IRequestTemplateSeederCoordinator
{
    /// <summary>
    /// Seeds all request templates from all registered seeders.
    /// </summary>
    /// <param name="creatorId">The ID of the user to set as the creator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total number of templates created.</returns>
    Task<int> SeedAllAsync(Guid creatorId, CancellationToken cancellationToken = default);
}
