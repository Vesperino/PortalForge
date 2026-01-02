using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

/// <summary>
/// Interface for category-specific request template seeders.
/// Each implementation handles seeding templates for a specific category (IT, HR, Training, etc.).
/// </summary>
public interface IRequestTemplateSeeder
{
    /// <summary>
    /// Gets the category name this seeder handles.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Seeds request templates for this category.
    /// </summary>
    /// <param name="creatorId">The ID of the user to set as the creator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created request templates.</returns>
    Task<IReadOnlyList<RequestTemplate>> SeedAsync(Guid creatorId, CancellationToken cancellationToken = default);
}
