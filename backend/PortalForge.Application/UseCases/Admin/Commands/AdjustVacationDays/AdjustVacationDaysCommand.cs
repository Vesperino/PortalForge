using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.AdjustVacationDays;

/// <summary>
/// Command to manually adjust user's vacation days allowance.
/// This is used by admins to correct vacation data or add/subtract days.
/// All adjustments are audited.
/// </summary>
public class AdjustVacationDaysCommand : IRequest<AdjustVacationDaysResult>, ITransactionalRequest
{
    public Guid UserId { get; set; }

    /// <summary>
    /// Amount to adjust (positive to add, negative to subtract)
    /// </summary>
    public int AdjustmentAmount { get; set; }

    /// <summary>
    /// Reason for the adjustment (required for audit trail)
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Admin user performing the adjustment
    /// </summary>
    public Guid AdjustedBy { get; set; }
}

public class AdjustVacationDaysResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int OldValue { get; set; }
    public int NewValue { get; set; }
}
