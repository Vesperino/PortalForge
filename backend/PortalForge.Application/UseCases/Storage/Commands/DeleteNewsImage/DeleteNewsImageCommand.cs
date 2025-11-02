using MediatR;

namespace PortalForge.Application.UseCases.Storage.Commands.DeleteNewsImage;

public class DeleteNewsImageCommand : IRequest<bool>
{
    public string FilePath { get; set; } = string.Empty;
    public Guid? DeletedBy { get; set; }
}
