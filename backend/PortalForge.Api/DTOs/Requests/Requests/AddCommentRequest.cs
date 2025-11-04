namespace PortalForge.Api.DTOs.Requests.Requests;

/// <summary>
/// DTO for adding a comment to a request
/// </summary>
public class AddCommentRequest
{
    public string Comment { get; set; } = string.Empty;
    public string? Attachments { get; set; }
}
