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
}

