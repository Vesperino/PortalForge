using PortalForge.Domain.Enums;

namespace PortalForge.Api.DTOs.Requests.Requests;

/// <summary>
/// DTO for updating service task status
/// </summary>
public class UpdateServiceTaskStatusDto
{
    public ServiceTaskStatus Status { get; set; }
    public string? Notes { get; set; }
}