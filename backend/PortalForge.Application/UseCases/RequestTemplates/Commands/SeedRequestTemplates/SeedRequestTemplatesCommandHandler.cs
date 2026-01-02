using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.SeedRequestTemplates;

public sealed class SeedRequestTemplatesCommandHandler
    : IRequestHandler<SeedRequestTemplatesCommand, SeedRequestTemplatesResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestTemplateSeederCoordinator _seederCoordinator;
    private readonly ILogger<SeedRequestTemplatesCommandHandler> _logger;

    public SeedRequestTemplatesCommandHandler(
        IUnitOfWork unitOfWork,
        IRequestTemplateSeederCoordinator seederCoordinator,
        ILogger<SeedRequestTemplatesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _seederCoordinator = seederCoordinator;
        _logger = logger;
    }

    public async Task<SeedRequestTemplatesResult> Handle(
        SeedRequestTemplatesCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting request templates seeding...");

        var existingTemplates = await _unitOfWork.RequestTemplateRepository.GetAllAsync();
        if (existingTemplates.Any())
        {
            _logger.LogInformation("Request templates already exist. Skipping seed.");
            return new SeedRequestTemplatesResult
            {
                TemplatesCreated = 0,
                Message = "Templates already exist"
            };
        }

        var users = await _unitOfWork.UserRepository.GetAllAsync();
        var adminUser = users.FirstOrDefault(u => u.Role == UserRole.Admin);
        if (adminUser == null)
        {
            _logger.LogWarning("No admin user found for seeding templates");
            return new SeedRequestTemplatesResult
            {
                TemplatesCreated = 0,
                Message = "No admin user found"
            };
        }

        var count = await _seederCoordinator.SeedAllAsync(adminUser.Id, cancellationToken);

        await _unitOfWork.SaveChangesAsync();

        var message = $"Created {count} request templates";
        _logger.LogInformation("{Message}", message);

        return new SeedRequestTemplatesResult
        {
            TemplatesCreated = count,
            Message = message
        };
    }
}
