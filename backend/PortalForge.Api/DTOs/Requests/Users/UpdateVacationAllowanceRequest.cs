namespace PortalForge.Api.DTOs.Requests.Users;

public class UpdateVacationAllowanceRequest
{
    public int NewAnnualDays { get; set; }
    public string Reason { get; set; } = string.Empty;
}
