using MediatR;

namespace PortalForge.Application.UseCases.ChatAI.Commands.TranslateText;

/// <summary>
/// Command to translate text using OpenAI.
/// Returns the complete translated text.
/// </summary>
public class TranslateTextCommand : IRequest<string>
{
    public string TextToTranslate { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
}
