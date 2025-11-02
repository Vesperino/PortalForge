namespace PortalForge.Application.UseCases.InternalServices.DTOs;

public class InternalServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string IconType { get; set; } = "emoji";
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsGlobal { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Guid> DepartmentIds { get; set; } = new();
}
