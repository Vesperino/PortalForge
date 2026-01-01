namespace PortalForge.Api.DTOs.Requests.Users;

public sealed class UpdateVacationAllowanceRequest
{
    public int NewAnnualDays { get; init; }
    public string Reason { get; init; } = string.Empty;
}
