using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.DeleteRequestTemplate;

public class DeleteRequestTemplateCommandHandler 
    : IRequestHandler<DeleteRequestTemplateCommand, DeleteRequestTemplateResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteRequestTemplateCommandHandler> _logger;

    public DeleteRequestTemplateCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteRequestTemplateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeleteRequestTemplateResult> Handle(
        DeleteRequestTemplateCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting request template: {TemplateId} by user: {UserId}", 
            request.Id, request.DeletedBy);

        // Get template
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(request.Id);
        if (template == null)
        {
            throw new NotFoundException($"Request template with ID {request.Id} not found");
        }

        // Check if template is being used by any requests
        var requests = await _unitOfWork.RequestRepository.GetByTemplateIdAsync(request.Id);

        // Store template data for audit
        var templateData = new
        {
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            FieldsCount = template.Fields.Count,
            ApprovalStepsCount = template.ApprovalStepTemplates.Count,
            HasAssociatedRequests = requests.Any(),
            RequestCount = requests.Count()
        };

        // If template has associated requests, soft-delete it (set IsActive = false)
        // Otherwise, hard-delete it
        if (requests.Any())
        {
            _logger.LogInformation(
                "Template has {Count} associated requests. Performing soft-delete (IsActive = false)",
                requests.Count());

            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RequestTemplateRepository.UpdateAsync(template);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            _logger.LogInformation("Template has no associated requests. Performing hard-delete");

            // Delete template (cascade delete will handle related entities)
            await _unitOfWork.RequestTemplateRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync();
        }

        // Log audit
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = request.DeletedBy,
            Action = "DeleteRequestTemplate",
            EntityType = "RequestTemplate",
            EntityId = template.Id.ToString(),
            OldValue = System.Text.Json.JsonSerializer.Serialize(templateData),
            NewValue = null,
            Timestamp = DateTime.UtcNow
        };

        await _unitOfWork.AuditLogRepository.CreateAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Request template deleted successfully: {TemplateId}", request.Id);

        var resultMessage = requests.Any()
            ? $"Template deactivated successfully. It had {requests.Count()} associated request(s) and was soft-deleted."
            : "Template deleted successfully";

        return new DeleteRequestTemplateResult
        {
            Success = true,
            Message = resultMessage
        };
    }
}

