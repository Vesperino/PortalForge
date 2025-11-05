using MediatR;

namespace PortalForge.Application.UseCases.Requests.Commands.BulkApproveRequests;

public class BulkApproveRequestsCommand : IRequest<BulkApproveRequestsResult>
{
    /// <summary>
    /// List of request step IDs to approve in bulk
    /// </summary>
    public List<Guid> RequestStepIds { get; set; } = new();

    /// <summary>
    /// ID of the user performing the bulk approval
    /// </summary>
    public Guid ApproverId { get; set; }

    /// <summary>
    /// Optional comment to apply to all approved steps
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Whether to skip validation checks (admin override)
    /// </summary>
    public bool SkipValidation { get; set; } = false;
}

public class BulkApproveRequestsResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SuccessfulApprovals { get; set; }
    public int FailedApprovals { get; set; }
    public List<BulkApprovalError> Errors { get; set; } = new();
}

public class BulkApprovalError
{
    public Guid RequestStepId { get; set; }
    public string RequestTitle { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}