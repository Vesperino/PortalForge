using MediatR;
using PortalForge.Application.UseCases.SystemSettings.Queries.GetAllSettings;

namespace PortalForge.Application.UseCases.SystemSettings.Queries.GetSettingByKey;

public class GetSettingByKeyQuery : IRequest<SystemSettingDto>
{
    public string Key { get; set; } = string.Empty;
}
