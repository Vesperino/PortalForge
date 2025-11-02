using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadServiceIcon;

public class UploadServiceIconCommand : IRequest<UploadServiceIconResult>, ITransactionalRequest
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
