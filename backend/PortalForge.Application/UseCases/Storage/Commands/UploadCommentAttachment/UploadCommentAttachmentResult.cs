namespace PortalForge.Application.UseCases.Storage.Commands.UploadCommentAttachment;

public class UploadCommentAttachmentResult
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
