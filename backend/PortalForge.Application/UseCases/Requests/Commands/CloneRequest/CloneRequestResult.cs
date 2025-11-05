namespace PortalForge.Application.UseCases.Requests.Commands.CloneRequest;

public class CloneRequestResult
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsTemplate { get; set; }
}