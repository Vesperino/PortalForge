namespace PortalForge.Api.DTOs.Requests.Requests;

/// <summary>
/// DTO for bulk approval of request steps
/// </summary>
public class BulkApproveRequestsDto
{
    public List<Guid> RequestStepIds { get; set; } = new();
    public string? Comment { get; set; }
    public bool SkipValidation { get; set; } = false;
}