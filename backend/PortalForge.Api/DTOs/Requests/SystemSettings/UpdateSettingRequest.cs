namespace PortalForge.Api.DTOs.Requests.SystemSettings;

public class UpdateSettingRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
