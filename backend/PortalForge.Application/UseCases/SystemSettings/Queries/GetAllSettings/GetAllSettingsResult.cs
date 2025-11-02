namespace PortalForge.Application.UseCases.SystemSettings.Queries.GetAllSettings;

public class GetAllSettingsResult
{
    public List<SystemSettingDto> Settings { get; set; } = new();
}
