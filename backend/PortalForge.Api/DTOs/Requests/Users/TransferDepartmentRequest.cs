namespace PortalForge.Api.DTOs.Requests.Users;

public sealed class TransferDepartmentRequest
{
    public Guid NewDepartmentId { get; init; }
    public Guid? NewSupervisorId { get; init; }
    public string? Reason { get; init; }
}
