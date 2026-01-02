using PortalForge.Domain.Entities;

namespace PortalForge.Application.Common.Interfaces;

public interface IVacationDaysDeductionService
{
    Task DeductVacationDaysAsync(Request request, CancellationToken cancellationToken = default);
}
