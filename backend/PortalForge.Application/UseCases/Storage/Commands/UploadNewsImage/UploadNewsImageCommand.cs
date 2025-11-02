using MediatR;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadNewsImage;

public class UploadNewsImageCommand : IRequest<UploadNewsImageResult>
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid? UploadedBy { get; set; }
}
