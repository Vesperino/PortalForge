using PortalForge.Application.DTOs;

namespace PortalForge.Application.Interfaces;

/// <summary>
/// Service for building dynamic forms from request templates and handling form validation
/// </summary>
public interface IFormBuilderService
{
    /// <summary>
    /// Builds a form definition from a request template
    /// </summary>
    /// <param name="templateId">The ID of the request template</param>
    /// <returns>A form definition containing all fields and their configurations</returns>
    Task<FormDefinitionDto> BuildFormAsync(Guid templateId);

    /// <summary>
    /// Validates form data against template validation rules
    /// </summary>
    /// <param name="templateId">The ID of the request template</param>
    /// <param name="formData">JSON string containing form field values</param>
    /// <returns>Validation result with any errors found</returns>
    Task<ValidationResult> ValidateFormDataAsync(Guid templateId, string formData);

    /// <summary>
    /// Processes conditional logic to determine field visibility
    /// </summary>
    /// <param name="templateId">The ID of the request template</param>
    /// <param name="formData">JSON string containing current form field values</param>
    /// <returns>JSON string containing field visibility states</returns>
    Task<string> ProcessConditionalLogicAsync(Guid templateId, string formData);

    /// <summary>
    /// Gets auto-complete options for intelligent field suggestions
    /// </summary>
    /// <param name="source">The auto-complete source identifier</param>
    /// <param name="query">The search query</param>
    /// <returns>List of auto-complete options</returns>
    Task<List<AutoCompleteOptionDto>> GetAutoCompleteOptionsAsync(string source, string query);
}