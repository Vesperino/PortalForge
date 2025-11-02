using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadNewsImage;

public class UploadNewsImageCommandHandler : IRequestHandler<UploadNewsImageCommand, UploadNewsImageResult>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadNewsImageCommandHandler> _logger;

    public UploadNewsImageCommandHandler(
        IFileStorageService fileStorageService,
        IUnifiedValidatorService validatorService,
        IUnitOfWork unitOfWork,
        ILogger<UploadNewsImageCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _validatorService = validatorService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UploadNewsImageResult> Handle(UploadNewsImageCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        // Get news images path from system settings
        var newsImagesPathSetting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync("Storage:NewsImagesPath");
        var newsImagesPath = newsImagesPathSetting?.Value ?? "images";

        _logger.LogInformation("Uploading news image: {FileName}, Size: {FileSize} bytes to {Path}",
            request.FileName, request.FileSize, newsImagesPath);

        var relativePath = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName, newsImagesPath);

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
