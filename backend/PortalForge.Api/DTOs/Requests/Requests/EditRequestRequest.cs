namespace PortalForge.Api.DTOs.Requests.Requests;

/// <summary>
/// DTO for editing an existing request
/// </summary>
public class EditRequestRequest
{
    public string FormData { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}
