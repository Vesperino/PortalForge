namespace PortalForge.Domain.Enums;

/// <summary>
/// Types of validation rules that can be applied to form fields
/// </summary>
public enum ValidationType
{
    Required,
    MinLength,
    MaxLength,
    Regex,
    Custom,
    Conditional,
    Range,
    FileSize,
    FileType,
    Email,
    Phone,
    Url,
    Date,
    Number
}