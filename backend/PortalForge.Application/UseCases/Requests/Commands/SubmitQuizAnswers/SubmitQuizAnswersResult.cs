namespace PortalForge.Application.UseCases.Requests.Commands.SubmitQuizAnswers;

public class SubmitQuizAnswersResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool Passed { get; set; }
    public int RequiredScore { get; set; }
}
