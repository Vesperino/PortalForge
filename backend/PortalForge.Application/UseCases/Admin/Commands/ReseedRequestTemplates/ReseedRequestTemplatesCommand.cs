using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.ReseedRequestTemplates;

/// <summary>
/// Command to force reseed default request templates.
/// Removes existing templates and recreates them with latest structure.
/// </summary>
public class ReseedRequestTemplatesCommand : IRequest<ReseedResult>, ITransactionalRequest
{
    public bool ForceRecreate { get; set; } = true;
}

public class ReseedResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TemplatesCreated { get; set; }
}
