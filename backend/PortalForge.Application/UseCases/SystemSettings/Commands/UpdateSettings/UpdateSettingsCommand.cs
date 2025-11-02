using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.SystemSettings.Commands.UpdateSettings;

public class UpdateSettingsCommand : IRequest<UpdateSettingsResult>, ITransactionalRequest
{
    public List<UpdateSettingDto> Settings { get; set; } = new();
    public Guid UpdatedBy { get; set; }
}

public class UpdateSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
