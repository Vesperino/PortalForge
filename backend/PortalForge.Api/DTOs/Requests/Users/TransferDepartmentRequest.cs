namespace PortalForge.Api.DTOs.Requests.Users;

public class TransferDepartmentRequest
{
    public Guid NewDepartmentId { get; set; }
    public Guid? NewSupervisorId { get; set; }
    public string? Reason { get; set; }
}
