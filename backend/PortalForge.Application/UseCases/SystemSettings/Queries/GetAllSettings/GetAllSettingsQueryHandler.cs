using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.SystemSettings.Queries.GetAllSettings;

public class GetAllSettingsQueryHandler : IRequestHandler<GetAllSettingsQuery, GetAllSettingsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllSettingsQueryHandler> _logger;

    public GetAllSettingsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllSettingsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetAllSettingsResult> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all system settings");

        var settings = await _unitOfWork.SystemSettingRepository.GetAllAsync();

        return new GetAllSettingsResult
        {
            Settings = settings.Select(s => new SystemSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Category = s.Category,
                Description = s.Description,
                UpdatedAt = s.UpdatedAt
            }).ToList()
        };
    }
}
