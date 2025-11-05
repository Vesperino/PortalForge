namespace PortalForge.Api.DTOs.Requests.Requests;

/// <summary>
/// DTO for cloning an existing request
/// </summary>
public class CloneRequestDto
{
    public string? ModifiedFormData { get; set; }
    public bool CreateAsTemplate { get; set; } = false;
}