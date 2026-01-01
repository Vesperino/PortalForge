namespace PortalForge.Api.DTOs.Requests.Vacations;

public sealed class CancelVacationRequest
{
    public string Reason { get; init; } = string.Empty;
}
