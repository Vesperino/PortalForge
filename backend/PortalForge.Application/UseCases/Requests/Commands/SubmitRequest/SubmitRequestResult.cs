namespace PortalForge.Application.UseCases.Requests.Commands.SubmitRequest;

public class SubmitRequestResult
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

