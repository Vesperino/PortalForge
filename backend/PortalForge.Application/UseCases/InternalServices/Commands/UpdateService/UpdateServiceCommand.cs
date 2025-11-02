using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.InternalServices.Commands.UpdateService;

public class UpdateServiceCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string IconType { get; set; } = "emoji";
    public Guid? CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsGlobal { get; set; }
    public bool IsPinned { get; set; }
    public List<Guid> DepartmentIds { get; set; } = new();
}
