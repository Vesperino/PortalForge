namespace PortalForge.Api.DTOs.Requests.ChatAI;

public sealed class TranslationResponse
{
    public string TranslatedText { get; init; } = string.Empty;
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
}
