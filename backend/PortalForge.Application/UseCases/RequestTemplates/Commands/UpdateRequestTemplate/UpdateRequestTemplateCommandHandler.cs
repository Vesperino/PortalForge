using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.RequestTemplates.Commands.UpdateRequestTemplate;

public class UpdateRequestTemplateCommandHandler 
    : IRequestHandler<UpdateRequestTemplateCommand, UpdateRequestTemplateResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateRequestTemplateCommandHandler> _logger;

    public UpdateRequestTemplateCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateRequestTemplateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateRequestTemplateResult> Handle(
        UpdateRequestTemplateCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating request template {TemplateId}", request.Id);

        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(request.Id);
        
        if (template == null)
        {
            _logger.LogWarning("Request template {TemplateId} not found", request.Id);
            return new UpdateRequestTemplateResult
            {
                Success = false,
                Message = "Template not found"
            };
        }

        // Update only provided fields
        if (request.Name != null)
            template.Name = request.Name;
        
        if (request.Description != null)
            template.Description = request.Description;
        
        if (request.Icon != null)
            template.Icon = request.Icon;
        
        if (request.Category != null)
            template.Category = request.Category;
        
        if (request.EstimatedProcessingDays.HasValue)
            template.EstimatedProcessingDays = request.EstimatedProcessingDays;
        
        if (request.IsActive.HasValue)
            template.IsActive = request.IsActive.Value;

        await _unitOfWork.RequestTemplateRepository.UpdateAsync(template);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Request template {TemplateId} updated successfully", request.Id);

        return new UpdateRequestTemplateResult
        {
            Success = true,
            Message = "Template updated successfully"
        };
    }
}

