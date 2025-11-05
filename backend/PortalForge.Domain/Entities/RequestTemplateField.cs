using PortalForge.Domain.Enums;

namespace PortalForge.Domain.Entities;

public class RequestTemplateField
{
    public Guid Id { get; set; }
    public Guid RequestTemplateId { get; set; }
    public RequestTemplate RequestTemplate { get; set; } = null!;
    
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; } = false;
    
    // JSON string for select options: [{value: "...", label: "..."}]
    public string? Options { get; set; }
    
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public string? HelpText { get; set; }
    public int Order { get; set; }
    
    // New properties for enhanced functionality
    /// <summary>
    /// JSON configuration for validation rules
    /// </summary>
    public string? ValidationRules { get; set; }
    
    /// <summary>
    /// JSON rules for showing/hiding fields based on other field values
    /// </summary>
    public string? ConditionalLogic { get; set; }
    
    /// <summary>
    /// Indicates if this field's visibility depends on other fields
    /// </summary>
    public bool IsConditional { get; set; } = false;
    
    /// <summary>
    /// Default value for the field
    /// </summary>
    public string? DefaultValue { get; set; }
    
    /// <summary>
    /// API endpoint or source for auto-complete suggestions
    /// </summary>
    public string? AutoCompleteSource { get; set; }
    
    /// <summary>
    /// Maximum file size in bytes for file upload fields
    /// </summary>
    public int? FileMaxSize { get; set; }
    
    /// <summary>
    /// JSON array of allowed file types for file upload fields
    /// </summary>
    public string? AllowedFileTypes { get; set; }
}

