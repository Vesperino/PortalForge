using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Storage.Commands.DeleteNewsImage;

public class DeleteNewsImageCommandHandler : IRequestHandler<DeleteNewsImageCommand, bool>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<DeleteNewsImageCommandHandler> _logger;

    public DeleteNewsImageCommandHandler(
        IFileStorageService fileStorageService,
        IUnifiedValidatorService validatorService,
        ILogger<DeleteNewsImageCommandHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteNewsImageCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Deleting news image: {FilePath}", request.FilePath);

        await _fileStorageService.DeleteFileAsync(request.FilePath);

        _logger.LogInformation("News image deleted successfully: {FilePath}", request.FilePath);

        return true;
    }
}
