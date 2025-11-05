using MediatR;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.ChatAI.Commands.TranslateText;

/// <summary>
/// Handler for translating text using OpenAI.
/// </summary>
public class TranslateTextCommandHandler : IRequestHandler<TranslateTextCommand, string>
{
    private readonly IOpenAIService _openAIService;
    private readonly IUnifiedValidatorService _validatorService;

    public TranslateTextCommandHandler(
        IOpenAIService openAIService,
        IUnifiedValidatorService validatorService)
    {
        _openAIService = openAIService;
        _validatorService = validatorService;
    }

    public async Task<string> Handle(
        TranslateTextCommand request,
        CancellationToken cancellationToken)
    {
        // Validate the request
        await _validatorService.ValidateAsync(request);

        // Get translation from OpenAI
        return await _openAIService.TranslateTextAsync(
            request.TextToTranslate,
            request.TargetLanguage,
            cancellationToken);
    }
}
