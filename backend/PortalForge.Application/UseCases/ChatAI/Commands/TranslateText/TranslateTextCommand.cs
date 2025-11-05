using MediatR;

namespace PortalForge.Application.UseCases.ChatAI.Commands.TranslateText;

/// <summary>
/// Command to translate text using OpenAI.
/// Returns an async enumerable for streaming the translation.
/// </summary>
public class TranslateTextCommand : IRequest<IAsyncEnumerable<string>>
{
    public string TextToTranslate { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
}
