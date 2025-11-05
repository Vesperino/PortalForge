using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.SystemSettings.Commands.UpdateSettings;

public class UpdateSettingsCommandHandler : IRequestHandler<UpdateSettingsCommand, UpdateSettingsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UpdateSettingsCommandHandler> _logger;

    public UpdateSettingsCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        IEncryptionService encryptionService,
        ILogger<UpdateSettingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<UpdateSettingsResult> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Updating {Count} system settings by user {UserId}",
            request.Settings.Count, request.UpdatedBy);

        int updatedCount = 0;

        foreach (var updateDto in request.Settings)
        {
            var setting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync(updateDto.Key);

            if (setting != null)
            {
                // Encrypt OpenAI API Key before saving
                if (updateDto.Key == "AI:OpenAIApiKey" && !string.IsNullOrWhiteSpace(updateDto.Value))
                {
                    _logger.LogInformation("Encrypting OpenAI API key before saving");
                    setting.Value = _encryptionService.Encrypt(updateDto.Value);
                }
                else
                {
                    setting.Value = updateDto.Value;
                }

                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = request.UpdatedBy;

                await _unitOfWork.SystemSettingRepository.UpdateAsync(setting);
                updatedCount++;
            }
            else
            {
                _logger.LogWarning("Setting with key '{Key}' not found, skipping update", updateDto.Key);
            }
        }

        _logger.LogInformation("Successfully updated {UpdatedCount} system settings", updatedCount);

        return new UpdateSettingsResult
        {
            UpdatedCount = updatedCount
        };
    }
}
