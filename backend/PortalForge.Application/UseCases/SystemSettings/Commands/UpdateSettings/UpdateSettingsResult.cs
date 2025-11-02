namespace PortalForge.Application.UseCases.SystemSettings.Commands.UpdateSettings;

public class UpdateSettingsResult
{
    public int UpdatedCount { get; set; }
    public string Message { get; set; } = "Settings updated successfully";
}
