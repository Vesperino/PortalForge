namespace PortalForge.Application.UseCases.InternalServices.DTOs;

public class CreateInternalServiceRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string IconType { get; set; } = "emoji";
    public Guid? CategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsGlobal { get; set; } = false;
    public bool IsPinned { get; set; } = false;
    public List<Guid> DepartmentIds { get; set; } = new();
}
