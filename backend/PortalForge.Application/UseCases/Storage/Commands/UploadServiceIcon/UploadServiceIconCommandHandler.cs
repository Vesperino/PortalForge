using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Storage.Commands.UploadServiceIcon;

public class UploadServiceIconCommandHandler : IRequestHandler<UploadServiceIconCommand, UploadServiceIconResult>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadServiceIconCommandHandler> _logger;

    public UploadServiceIconCommandHandler(
        IFileStorageService fileStorageService,
        ILogger<UploadServiceIconCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<UploadServiceIconResult> Handle(UploadServiceIconCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading service icon: {FileName}, Size: {FileSize}", request.FileName, request.FileSize);

        var relativePath = await _fileStorageService.SaveFileAsync(
            request.FileStream,
            request.FileName,
            "service-icons");

        _logger.LogInformation("Service icon uploaded successfully: {FilePath}", relativePath);

        return new UploadServiceIconResult
        {
            FilePath = relativePath,
            FileName = request.FileName
        };
    }
}
