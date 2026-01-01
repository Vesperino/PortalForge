namespace PortalForge.Api.DTOs.Requests.Vacations;

public sealed class ValidateVacationResponse
{
    public bool CanTake { get; init; }
    public string? ErrorMessage { get; init; }
    public int RequestedDays { get; init; }
}
