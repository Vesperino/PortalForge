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
        if (requests.Any())
        {
            throw new ValidationException(
                "Cannot delete template that is being used by requests", 
                new List<string> { $"This template is used by {requests.Count()} request(s)" });
        }

        // Store template data for audit
        var templateData = new
        {
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            FieldsCount = template.Fields.Count,
            ApprovalStepsCount = template.ApprovalStepTemplates.Count
        };

        // Delete template (cascade delete will handle related entities)
        await _unitOfWork.RequestTemplateRepository.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync();

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

        return new DeleteRequestTemplateResult
        {
            Success = true,
            Message = "Template deleted successfully"
        };
    }
}

