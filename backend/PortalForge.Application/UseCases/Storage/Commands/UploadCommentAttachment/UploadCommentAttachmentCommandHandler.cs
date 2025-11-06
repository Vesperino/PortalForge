using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadCommentAttachment;

public class UploadCommentAttachmentCommandHandler : IRequestHandler<UploadCommentAttachmentCommand, UploadCommentAttachmentResult>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadCommentAttachmentCommandHandler> _logger;

    public UploadCommentAttachmentCommandHandler(
        IFileStorageService fileStorageService,
        IUnifiedValidatorService validatorService,
        IUnitOfWork unitOfWork,
        ILogger<UploadCommentAttachmentCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _validatorService = validatorService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UploadCommentAttachmentResult> Handle(UploadCommentAttachmentCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        // Get comment attachments path from system settings
        var commentAttachmentsPathSetting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync("Storage:CommentAttachmentsPath");
        var commentAttachmentsPath = commentAttachmentsPathSetting?.Value ?? "comment-attachments";

        _logger.LogInformation("Uploading comment attachment: {FileName}, Size: {FileSize} bytes to {Path}",
            request.FileName, request.FileSize, commentAttachmentsPath);

        var relativePath = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName, commentAttachmentsPath);

        var fileName = Path.GetFileName(relativePath);

        _logger.LogInformation("Comment attachment uploaded successfully: {RelativePath}", relativePath);

        return new UploadCommentAttachmentResult
        {
            FileName = fileName,
            FilePath = relativePath,
            Url = $"/api/storage/files/{relativePath}"
        };
    }
}
