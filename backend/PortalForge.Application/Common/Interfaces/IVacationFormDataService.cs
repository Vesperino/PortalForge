using PortalForge.Domain.Enums;

namespace PortalForge.Application.Common.Interfaces;

public record VacationFormData(
    DateTime StartDate,
    DateTime EndDate,
    LeaveType LeaveType);

public interface IVacationFormDataService
{
    VacationFormData? ExtractVacationData(string formDataJson);

    int CalculateBusinessDays(DateTime startDate, DateTime endDate);
}
