namespace PortalForge.Application.UseCases.Requests.Commands.ApproveRequestStep;

public class ApproveRequestStepResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ApproveRequestStepResult Success(string message) =>
        new() { IsSuccess = true, Message = message };

    public static ApproveRequestStepResult Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}
