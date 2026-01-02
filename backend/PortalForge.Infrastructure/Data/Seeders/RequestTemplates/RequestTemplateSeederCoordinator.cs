using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Infrastructure.Data.Seeders.RequestTemplates;

/// <summary>
/// Coordinates all request template seeders and saves created templates to the database.
/// </summary>
public sealed class RequestTemplateSeederCoordinator : IRequestTemplateSeederCoordinator
{
    private readonly IEnumerable<IRequestTemplateSeeder> _seeders;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RequestTemplateSeederCoordinator> _logger;

    public RequestTemplateSeederCoordinator(
        IEnumerable<IRequestTemplateSeeder> seeders,
        IUnitOfWork unitOfWork,
        ILogger<RequestTemplateSeederCoordinator> logger)
    {
        _seeders = seeders;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> SeedAllAsync(Guid creatorId, CancellationToken cancellationToken = default)
    {
        var totalCount = 0;

        foreach (var seeder in _seeders)
        {
            _logger.LogInformation("Running seeder for category: {Category}", seeder.Category);

            try
            {
                var templates = await seeder.SeedAsync(creatorId, cancellationToken);

                foreach (var template in templates)
                {
                    await _unitOfWork.RequestTemplateRepository.CreateAsync(template);
                    totalCount++;
                    _logger.LogDebug("Created template: {TemplateName} ({Category})",
                        template.Name, template.Category);
                }

                _logger.LogInformation(
                    "Seeder for category {Category} completed. Created {Count} template(s)",
                    seeder.Category,
                    templates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error running seeder for category {Category}",
                    seeder.Category);
                throw;
            }
        }

        return totalCount;
    }
}
