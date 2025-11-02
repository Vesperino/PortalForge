using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadNewsImage;

public class UploadNewsImageCommandHandler : IRequestHandler<UploadNewsImageCommand, UploadNewsImageResult>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<UploadNewsImageCommandHandler> _logger;

    public UploadNewsImageCommandHandler(
        IFileStorageService fileStorageService,
        IUnifiedValidatorService validatorService,
        ILogger<UploadNewsImageCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<UploadNewsImageResult> Handle(UploadNewsImageCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Uploading news image: {FileName}, Size: {FileSize} bytes", request.FileName, request.FileSize);

        var relativePath = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName, "news-images");

        var fileName = Path.GetFileName(relativePath);

        _logger.LogInformation("News image uploaded successfully: {RelativePath}", relativePath);

        return new UploadNewsImageResult
        {
            FileName = fileName,
            FilePath = relativePath,
            // URL will be built in controller with HttpContext
            Url = $"/api/storage/files/{relativePath}"
        };
    }
}
