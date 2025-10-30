using PortalForge.Application.UseCases.RequestTemplates.DTOs;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplates;

public class GetRequestTemplatesResult
{
    public List<RequestTemplateDto> Templates { get; set; } = new();
}

