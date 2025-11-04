namespace PortalForge.Application.UseCases.Requests.DTOs;

public class ApprovalsHistoryItemDto
{
    public Guid RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateIcon { get; set; } = string.Empty;
    public string SubmittedByName { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public Guid StepId { get; set; }
    public int StepOrder { get; set; }
    public string Decision { get; set; } = string.Empty; // Approved/Rejected
    public DateTime? FinishedAt { get; set; }
    public string? Comment { get; set; }
}

