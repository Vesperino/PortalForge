using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.UseCases.SystemSettings.Queries.GetAllSettings;

namespace PortalForge.Application.UseCases.SystemSettings.Queries.GetSettingByKey;

public class GetSettingByKeyQueryHandler : IRequestHandler<GetSettingByKeyQuery, SystemSettingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<GetSettingByKeyQueryHandler> _logger;

    public GetSettingByKeyQueryHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<GetSettingByKeyQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<SystemSettingDto> Handle(GetSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        await _validatorService.ValidateAsync(request);

        _logger.LogInformation("Getting system setting by key: {Key}", request.Key);

        var setting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync(request.Key);

        if (setting == null)
        {
            throw new NotFoundException($"Setting with key '{request.Key}' not found");
        }

        return new SystemSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Category = setting.Category,
            Description = setting.Description,
            UpdatedAt = setting.UpdatedAt
        };
    }
}
