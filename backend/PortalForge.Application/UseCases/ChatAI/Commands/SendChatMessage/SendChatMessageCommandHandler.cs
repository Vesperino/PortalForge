using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.ChatAI.Commands.SendChatMessage;

/// <summary>
/// Handler for sending chat messages to OpenAI.
/// </summary>
public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, string>
{
    private readonly IOpenAIService _openAIService;
    private readonly IUnifiedValidatorService _validatorService;

    public SendChatMessageCommandHandler(
        IOpenAIService openAIService,
        IUnifiedValidatorService validatorService)
    {
        _openAIService = openAIService;
        _validatorService = validatorService;
    }

    public async Task<string> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        // Validate the request
        await _validatorService.ValidateAsync(request);

        // Get chat response from OpenAI
        return await _openAIService.ChatAsync(
            request.Message,
            request.ConversationHistory,
            cancellationToken);
    }
}
