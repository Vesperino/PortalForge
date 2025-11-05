using PortalForge.Domain.Enums;

namespace PortalForge.Application.DTOs;

/// <summary>
/// Represents a complete form definition built from a request template
/// </summary>
public class FormDefinitionDto
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool AllowsAttachments { get; set; }
    public List<FormFieldDto> Fields { get; set; } = new();
}

/// <summary>
/// Represents a single form field with all its configuration
/// </summary>
public class FormFieldDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
    public string? Options { get; set; } // JSON string for select options
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string? HelpText { get; set; }
    public int Order { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsConditional { get; set; }
    public string? AutoCompleteSource { get; set; }
    public int? FileMaxSize { get; set; }
    public string? AllowedFileTypes { get; set; }
    
    // Validation and conditional logic (parsed from JSON)
    public List<ValidationRuleDto> ValidationRules { get; set; } = new();
    public List<ConditionalLogicDto> ConditionalLogic { get; set; } = new();
}

/// <summary>
/// Represents a validation rule for a form field
/// </summary>
public class ValidationRuleDto
{
    public ValidationType Type { get; set; }
    public string? Value { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Represents conditional logic for showing/hiding fields
/// </summary>
public class ConditionalLogicDto
{
    public string FieldId { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty; // equals, not_equals, contains, etc.
    public string Value { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // show, hide, require, etc.
}

/// <summary>
/// Represents an auto-complete option
/// </summary>
public class AutoCompleteOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Validation result for form data
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<ValidationError> Errors { get; set; } = new();
}

/// <summary>
/// Represents a validation error
/// </summary>
public class ValidationError
{
    public string FieldId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationType Type { get; set; }
}