using System.Text.Json;
using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Requests.Commands.SubmitQuizAnswers;

public class SubmitQuizAnswersCommandHandler
    : IRequestHandler<SubmitQuizAnswersCommand, SubmitQuizAnswersResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitQuizAnswersCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmitQuizAnswersResult> Handle(
        SubmitQuizAnswersCommand command,
        CancellationToken cancellationToken)
    {
        // Get request with approval steps
        var request = await _unitOfWork.RequestRepository.GetByIdAsync(command.RequestId);
        if (request == null)
        {
            return new SubmitQuizAnswersResult
            {
                Success = false,
                Message = "Request not found"
            };
        }

        // Get the specific approval step
        var step = request.ApprovalSteps.FirstOrDefault(s => s.Id == command.StepId);
        if (step == null)
        {
            return new SubmitQuizAnswersResult
            {
                Success = false,
                Message = "Approval step not found"
            };
        }

        // Verify user is the request submitter (quiz is filled by submitter, not approver)
        if (request.SubmittedById != command.ApproverId) // Note: ApproverId is actually UserId here (naming issue)
        {
            return new SubmitQuizAnswersResult
            {
                Success = false,
                Message = "Unauthorized: Only the request submitter can fill the quiz"
            };
        }

        // Verify quiz is required
        if (!step.RequiresQuiz)
        {
            return new SubmitQuizAnswersResult
            {
                Success = false,
                Message = "Quiz is not required for this step"
            };
        }

        // Check if quiz was already submitted (one attempt only)
        if (step.QuizScore.HasValue)
        {
            return new SubmitQuizAnswersResult
            {
                Success = false,
                Message = step.QuizPassed == true
                    ? "Quiz already completed and passed. You can approve this request."
                    : $"Quiz already completed. You scored {step.QuizScore}% which did not meet the required threshold. This request cannot be approved.",
                Score = step.QuizScore.Value,
                Passed = step.QuizPassed ?? false,
                RequiredScore = step.PassingScore ?? 70
            };
        }

        // Get quiz questions from template
        var template = await _unitOfWork.RequestTemplateRepository.GetByIdAsync(request.RequestTemplateId);
        if (template == null)
        {
            return new SubmitQuizAnswersResult
            {
                Success = false,
                Message = "Request template not found"
            };
        }

        var stepTemplate = template.ApprovalStepTemplates.FirstOrDefault(
            ast => ast.Id == step.RequestApprovalStepTemplateId);

        if (stepTemplate == null || !stepTemplate.QuizQuestions.Any())
        {
            return new SubmitQuizAnswersResult
            {
                Success = false,
                Message = "Quiz questions not found for this step"
            };
        }

        // Calculate score
        int correctAnswers = 0;
        int totalQuestions = stepTemplate.QuizQuestions.Count;

        // Save answers and calculate score
        foreach (var answer in command.Answers)
        {
            var question = stepTemplate.QuizQuestions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) continue;

            // Parse options to find correct answer
            var options = JsonSerializer.Deserialize<List<QuizOption>>(question.Options);
            if (options == null) continue;

            var correctOption = options.FirstOrDefault(o => o.IsCorrect);
            var isCorrect = correctOption != null && correctOption.Value == answer.SelectedAnswer;

            if (isCorrect)
            {
                correctAnswers++;
            }

            // Save quiz answer
            var quizAnswer = new QuizAnswer
            {
                Id = Guid.NewGuid(),
                RequestApprovalStepId = step.Id,
                QuizQuestionId = question.Id,
                SelectedAnswer = answer.SelectedAnswer,
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.UtcNow
            };

            step.QuizAnswers.Add(quizAnswer);
        }

        // Calculate percentage score
        int percentageScore = totalQuestions > 0
            ? (int)Math.Round((double)correctAnswers / totalQuestions * 100)
            : 0;

        int requiredScore = step.PassingScore ?? stepTemplate.PassingScore ?? 70;
        bool passed = percentageScore >= requiredScore;

        // Update step with quiz results
        step.QuizScore = percentageScore;
        step.QuizPassed = passed;

        await _unitOfWork.RequestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return new SubmitQuizAnswersResult
        {
            Success = true,
            Message = passed
                ? "Quiz passed! You can now approve this request."
                : $"Quiz failed. You scored {percentageScore}% but need {requiredScore}% to pass.",
            Score = percentageScore,
            Passed = passed,
            RequiredScore = requiredScore
        };
    }
}

// Helper class for deserializing quiz options
public class QuizOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
