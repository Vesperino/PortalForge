namespace PortalForge.Api.DTOs.Requests.Users;

public sealed class BulkAssignDepartmentRequest
{
    public List<Guid> EmployeeIds { get; init; } = new();
    public Guid DepartmentId { get; init; }
}
