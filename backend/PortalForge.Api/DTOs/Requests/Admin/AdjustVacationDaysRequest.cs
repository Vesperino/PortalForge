namespace PortalForge.Api.DTOs.Requests.Admin;

public sealed class AdjustVacationDaysRequest
{
    public int AdjustmentAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
}
