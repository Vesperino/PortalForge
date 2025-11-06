using MediatR;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadCommentAttachment;

public class UploadCommentAttachmentCommand : IRequest<UploadCommentAttachmentResult>
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid? UploadedBy { get; set; }
}
