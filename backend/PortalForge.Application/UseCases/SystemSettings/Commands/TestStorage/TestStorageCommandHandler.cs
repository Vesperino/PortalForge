using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.SystemSettings.Commands.TestStorage;

public class TestStorageCommandHandler : IRequestHandler<TestStorageCommand, StorageTestResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageTestingService _storageTestingService;
    private readonly ILogger<TestStorageCommandHandler> _logger;

    public TestStorageCommandHandler(
        IUnitOfWork unitOfWork,
        IStorageTestingService storageTestingService,
        ILogger<TestStorageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _storageTestingService = storageTestingService;
        _logger = logger;
    }

    public async Task<StorageTestResult> Handle(TestStorageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Testing storage paths");

        // Get storage settings from database
        var storageSettings = await _unitOfWork.SystemSettingRepository.GetByCategoryAsync("Storage");

        var settingsDict = storageSettings.ToDictionary(s => s.Key, s => s.Value);

        // Test storage paths
        var result = await _storageTestingService.TestStoragePathsAsync(settingsDict);

        _logger.LogInformation("Storage test completed. Success: {Success}", result.Success);

        return result;
    }
}
