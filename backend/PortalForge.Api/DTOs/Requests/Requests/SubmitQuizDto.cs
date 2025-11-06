namespace PortalForge.Api.DTOs.Requests.Requests;

public class SubmitQuizDto
{
    public List<QuizAnswerDto> Answers { get; set; } = new();
}

public class QuizAnswerDto
{
    public Guid QuestionId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
}
