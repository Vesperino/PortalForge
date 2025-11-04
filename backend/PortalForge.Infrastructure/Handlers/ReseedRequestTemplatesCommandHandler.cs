using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortalForge.Application.UseCases.Admin.Commands.ReseedRequestTemplates;
using PortalForge.Infrastructure.Data.Seeders;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Infrastructure.Handlers;

/// <summary>
/// Handler for reseeding default request templates.
/// Removes old templates and creates new ones with updated structure.
/// </summary>
public class ReseedRequestTemplatesCommandHandler : IRequestHandler<ReseedRequestTemplatesCommand, ReseedResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReseedRequestTemplatesCommandHandler> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public ReseedRequestTemplatesCommandHandler(
        ApplicationDbContext context,
        ILogger<ReseedRequestTemplatesCommandHandler> logger,
        ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<ReseedResult> Handle(ReseedRequestTemplatesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting request templates reseed");

            if (request.ForceRecreate)
            {
                // Update existing templates instead of removing them (to preserve existing requests)
                var existingTemplates = await _context.RequestTemplates
                    .Where(t => t.Name == "Wniosek urlopowy" || t.Name == "Zgłoszenie L4 (zwolnienie lekarskie)")
                    .ToListAsync(cancellationToken);

                if (existingTemplates.Any())
                {
                    _logger.LogInformation("Found {Count} existing templates", existingTemplates.Count);

                    // If there are duplicates, keep only the first one
                    var vacationTemplates = existingTemplates.Where(t => t.Name == "Wniosek urlopowy").ToList();
                    var sickLeaveTemplates = existingTemplates.Where(t => t.Name == "Zgłoszenie L4 (zwolnienie lekarskie)").ToList();

                    // Remove duplicate vacation templates (keep first, remove others)
                    if (vacationTemplates.Count > 1)
                    {
                        _logger.LogWarning("Found {Count} vacation templates. Removing duplicates...", vacationTemplates.Count);
                        var toRemove = vacationTemplates.Skip(1).ToList();
                        _context.RequestTemplates.RemoveRange(toRemove);
                    }

                    // Remove duplicate sick leave templates (keep first, remove others)
                    if (sickLeaveTemplates.Count > 1)
                    {
                        _logger.LogWarning("Found {Count} sick leave templates. Removing duplicates...", sickLeaveTemplates.Count);
                        var toRemove = sickLeaveTemplates.Skip(1).ToList();
                        _context.RequestTemplates.RemoveRange(toRemove);
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    // Reload templates after removing duplicates
                    existingTemplates = await _context.RequestTemplates
                        .Where(t => t.Name == "Wniosek urlopowy" || t.Name == "Zgłoszenie L4 (zwolnienie lekarskie)")
                        .ToListAsync(cancellationToken);

                    // Remove only fields and steps (they will be recreated by seeder)
                    foreach (var template in existingTemplates)
                    {
                        // Remove ALL fields
                        var fields = await _context.RequestTemplateFields
                            .Where(f => f.RequestTemplateId == template.Id)
                            .ToListAsync(cancellationToken);

                        if (fields.Any())
                        {
                            _logger.LogInformation("Removing {Count} fields from template {TemplateName}", fields.Count, template.Name);
                            _context.RequestTemplateFields.RemoveRange(fields);
                        }

                        // Remove approval step templates
                        var steps = await _context.RequestApprovalStepTemplates
                            .Where(s => s.RequestTemplateId == template.Id)
                            .ToListAsync(cancellationToken);

                        if (steps.Any())
                        {
                            _logger.LogInformation("Removing {Count} steps from template {TemplateName}", steps.Count, template.Name);
                            _context.RequestApprovalStepTemplates.RemoveRange(steps);
                        }

                        // Mark templates as inactive temporarily (will be reactivated by seeder)
                        template.IsActive = false;
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Cleared fields and steps from {Count} templates", existingTemplates.Count);
                }
            }

            // Run seeder
            var seederLogger = _loggerFactory.CreateLogger<DefaultRequestTemplatesSeeder>();
            var seeder = new DefaultRequestTemplatesSeeder(_context, seederLogger);
            await seeder.SeedAsync();

            return new ReseedResult
            {
                Success = true,
                Message = "Szablony zostały pomyślnie zaktualizowane",
                TemplatesCreated = 2
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reseeding request templates");
            return new ReseedResult
            {
                Success = false,
                Message = $"Błąd podczas aktualizacji szablonów: {ex.Message}",
                TemplatesCreated = 0
            };
        }
    }
}
