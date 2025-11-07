namespace PortalForge.Api.DTOs.Requests.Users;

public class BulkAssignDepartmentRequest
{
    public List<Guid> EmployeeIds { get; set; } = new();
    public Guid DepartmentId { get; set; }
}
