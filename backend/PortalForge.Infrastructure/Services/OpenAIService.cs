using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Interfaces;
using AppChatMessage = PortalForge.Application.Interfaces.ChatMessage;
using OpenAIChatMessage = OpenAI.Chat.ChatMessage;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Service for integrating with OpenAI API for chat and translation features.
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        ILogger<OpenAIService> logger)
    {
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a translation request to OpenAI and returns the complete response.
    /// </summary>
    public async Task<string> TranslateTextAsync(
        string textToTranslate,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetDecryptedApiKeyAsync();
        var model = await GetTranslationModelAsync();

        var client = new ChatClient(model, apiKey);

        var systemPrompt = $@"You are a professional translator. Translate the following text to {targetLanguage}.
Maintain the original meaning, tone, and formatting.
Only provide the translation, no explanations or additional text.
If the text contains technical terms, preserve them accurately.";

        var messages = new List<OpenAIChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(textToTranslate)
        };

        var response = await client.CompleteChatAsync(messages, cancellationToken: cancellationToken);

        return response.Value.Content[0].Text;
    }

    /// <summary>
    /// Sends a chat message to OpenAI and returns the complete response.
    /// </summary>
    public async Task<string> ChatAsync(
        string message,
        IEnumerable<AppChatMessage>? conversationHistory = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetDecryptedApiKeyAsync();
        var model = await GetChatModelAsync();

        var client = new ChatClient(model, apiKey);

        var messages = new List<OpenAIChatMessage>();

        // Add conversation history if provided
        if (conversationHistory != null)
        {
            foreach (var historyMessage in conversationHistory)
            {
                var openAiMessage = historyMessage.Role.ToLower() switch
                {
                    "system" => (OpenAIChatMessage)new SystemChatMessage(historyMessage.Content),
                    "assistant" => (OpenAIChatMessage)new AssistantChatMessage(historyMessage.Content),
                    _ => (OpenAIChatMessage)new UserChatMessage(historyMessage.Content)
                };
                messages.Add(openAiMessage);
            }
        }

        // Add current user message
        messages.Add(new UserChatMessage(message));

        var response = await client.CompleteChatAsync(messages, cancellationToken: cancellationToken);

        return response.Value.Content[0].Text;
    }

    /// <summary>
    /// Tests the OpenAI API connection with the provided API key.
    /// </summary>
    public async Task<bool> TestConnectionAsync(string apiKey)
    {
        try
        {
            var client = new ChatClient("gpt-3.5-turbo", apiKey);

            var messages = new List<OpenAIChatMessage>
            {
                new UserChatMessage("Test")
            };

            var response = await client.CompleteChatAsync(messages);

            return response?.Value != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test OpenAI API connection");
            return false;
        }
    }

    /// <summary>
    /// Retrieves and decrypts the OpenAI API key from system settings.
    /// </summary>
    private async Task<string> GetDecryptedApiKeyAsync()
    {
        var setting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync("AI:OpenAIApiKey")
            ?? throw new InvalidOperationException("OpenAI API key is not configured in system settings.");

        if (string.IsNullOrWhiteSpace(setting.Value))
            throw new InvalidOperationException("OpenAI API key is empty. Please configure it in admin settings.");

        try
        {
            return _encryptionService.Decrypt(setting.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt OpenAI API key");
            throw new InvalidOperationException("Failed to decrypt OpenAI API key. The key may be corrupted.");
        }
    }

    /// <summary>
    /// Retrieves the translation model from system settings.
    /// </summary>
    private async Task<string> GetTranslationModelAsync()
    {
        var setting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync("AI:TranslationModel");
        return setting?.Value ?? "gpt-4";
    }

    /// <summary>
    /// Retrieves the chat model from system settings.
    /// </summary>
    private async Task<string> GetChatModelAsync()
    {
        var setting = await _unitOfWork.SystemSettingRepository.GetByKeyAsync("AI:ChatModel");
        return setting?.Value ?? "gpt-4";
    }
}
