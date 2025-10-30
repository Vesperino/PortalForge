using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetAvailableRequestTemplates;

public class GetAvailableRequestTemplatesResult
{
    public List<RequestTemplateDto> Templates { get; set; } = new();
}

