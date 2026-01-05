using MediatR;

namespace PortalForge.Application.UseCases.RequestTemplates.Queries.GetRequestTemplates;

public class GetRequestTemplatesQuery : IRequest<GetRequestTemplatesResult>
{
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

