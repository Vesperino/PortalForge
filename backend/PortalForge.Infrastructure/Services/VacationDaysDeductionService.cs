using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

public class VacationDaysDeductionService : IVacationDaysDeductionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVacationFormDataService _formDataService;
    private readonly ILogger<VacationDaysDeductionService> _logger;

    public VacationDaysDeductionService(
        IUnitOfWork unitOfWork,
        IVacationFormDataService formDataService,
        ILogger<VacationDaysDeductionService> logger)
    {
        _unitOfWork = unitOfWork;
        _formDataService = formDataService;
        _logger = logger;
    }

    public async Task DeductVacationDaysAsync(Request request, CancellationToken cancellationToken = default)
    {
        if (request.RequestTemplate?.IsVacationRequest != true)
        {
            return;
        }

        var vacationData = _formDataService.ExtractVacationData(request.FormData);
        if (vacationData == null)
        {
            _logger.LogWarning(
                "Could not extract vacation data from request {RequestId}",
                request.Id);
            return;
        }

        if (!IsDeductibleLeaveType(vacationData.LeaveType))
        {
            return;
        }

        var daysUsed = _formDataService.CalculateBusinessDays(
            vacationData.StartDate,
            vacationData.EndDate);

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.SubmittedById);
        if (user == null)
        {
            _logger.LogWarning(
                "User {UserId} not found for vacation deduction",
                request.SubmittedById);
            return;
        }

        UpdateUserVacationBalance(user, vacationData.LeaveType, daysUsed);
        await _unitOfWork.UserRepository.UpdateAsync(user);

        _logger.LogInformation(
            "Deducted {DaysUsed} {LeaveType} days from user {UserId} for request {RequestId}",
            daysUsed, vacationData.LeaveType, user.Id, request.Id);
    }

    private static bool IsDeductibleLeaveType(LeaveType leaveType)
    {
        return leaveType is LeaveType.Annual or LeaveType.OnDemand or LeaveType.Circumstantial;
    }

    private static void UpdateUserVacationBalance(User user, LeaveType leaveType, int daysUsed)
    {
        switch (leaveType)
        {
            case LeaveType.Annual:
                user.VacationDaysUsed = (user.VacationDaysUsed ?? 0) + daysUsed;
                break;

            case LeaveType.OnDemand:
                user.OnDemandVacationDaysUsed = (user.OnDemandVacationDaysUsed ?? 0) + daysUsed;
                user.VacationDaysUsed = (user.VacationDaysUsed ?? 0) + daysUsed;
                break;

            case LeaveType.Circumstantial:
                user.CircumstantialLeaveDaysUsed = (user.CircumstantialLeaveDaysUsed ?? 0) + daysUsed;
                break;
        }
    }
}
