using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitQuizAnswers;

public class SubmitQuizAnswersCommand : IRequest<SubmitQuizAnswersResult>, ITransactionalRequest
{
    public Guid RequestId { get; set; }
    public Guid StepId { get; set; }
    public Guid ApproverId { get; set; }
    public List<QuizAnswerSubmission> Answers { get; set; } = new();
}

public class QuizAnswerSubmission
{
    public Guid QuestionId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
}
