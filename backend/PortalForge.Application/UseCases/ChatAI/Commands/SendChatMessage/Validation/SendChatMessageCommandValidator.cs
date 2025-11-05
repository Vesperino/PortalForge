using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.ChatAI.Commands.SendChatMessage.Validation;

/// <summary>
/// Validator for SendChatMessageCommand.
/// </summary>
public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public SendChatMessageCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(10000).WithMessage("Message cannot exceed 10,000 characters");

        RuleFor(x => x.ConversationHistory)
            .Must(history => history == null || history.Count <= 50)
            .WithMessage("Conversation history cannot exceed 50 messages");
    }
}
