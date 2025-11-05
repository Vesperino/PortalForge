using FluentValidation;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.ChatAI.Commands.TranslateText.Validation;

/// <summary>
/// Validator for TranslateTextCommand.
/// </summary>
public class TranslateTextCommandValidator : AbstractValidator<TranslateTextCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public TranslateTextCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.TextToTranslate)
            .NotEmpty().WithMessage("Text to translate is required")
            .MustAsync(BeWithinCharacterLimit)
            .WithMessage("Text exceeds maximum character limit. Please check AI settings for the limit.");

        RuleFor(x => x.TargetLanguage)
            .NotEmpty().WithMessage("Target language is required")
            .MaximumLength(50).WithMessage("Target language cannot exceed 50 characters");
    }

    private async Task<bool> BeWithinCharacterLimit(string text, CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.SystemSettingRepository
            .GetByKeyAsync("AI:TranslationMaxCharacters");

        if (setting == null || !int.TryParse(setting.Value, out int maxChars))
        {
            maxChars = 8000; // Default if not configured
        }

        return text.Length <= maxChars;
    }
}
