using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadUserAvatar;

public class UploadUserAvatarCommandHandler : IRequestHandler<UploadUserAvatarCommand, UploadUserAvatarResult>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadUserAvatarCommandHandler> _logger;

    public UploadUserAvatarCommandHandler(
        IFileStorageService fileStorageService,
        IUnifiedValidatorService validatorService,
        IUnitOfWork unitOfWork,
        ILogger<UploadUserAvatarCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _validatorService = validatorService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UploadUserAvatarResult> Handle(UploadUserAvatarCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        // Get user avatars path from system settings
        var userAvatarsPathSetting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync("Storage:UserAvatarsPath");
        var userAvatarsPath = userAvatarsPathSetting?.Value ?? "user-avatars";

        _logger.LogInformation("Uploading user avatar: {FileName}, Size: {FileSize} bytes for user {UserId}",
            request.FileName, request.FileSize, request.UploadedBy);

        var relativePath = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName, userAvatarsPath);

        var fileName = Path.GetFileName(relativePath);

        _logger.LogInformation("User avatar uploaded successfully: {RelativePath}", relativePath);

        return new UploadUserAvatarResult
        {
            FileName = fileName,
            FilePath = relativePath,
            // URL will be built in controller with HttpContext
            Url = $"/api/storage/files/{relativePath}"
        };
    }
}
