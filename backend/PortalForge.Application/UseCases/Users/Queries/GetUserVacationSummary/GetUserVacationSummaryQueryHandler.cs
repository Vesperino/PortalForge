using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Users.Queries.GetUserVacationSummary;

/// <summary>
/// Handler for GetUserVacationSummaryQuery.
/// Retrieves user's vacation allowance, used days, and remaining days.
/// </summary>
public class GetUserVacationSummaryQueryHandler : IRequestHandler<GetUserVacationSummaryQuery, VacationSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUserVacationSummaryQueryHandler> _logger;

    public GetUserVacationSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUserVacationSummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<VacationSummaryDto> Handle(
        GetUserVacationSummaryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving vacation summary for user {UserId}",
            request.UserId);

        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        return new VacationSummaryDto
        {
            AnnualVacationDays = user.AnnualVacationDays,
            VacationDaysUsed = user.VacationDaysUsed,
            VacationDaysRemaining = user.AnnualVacationDays - user.VacationDaysUsed,
            OnDemandVacationDaysUsed = user.OnDemandVacationDaysUsed,
            OnDemandVacationDaysRemaining = 4 - user.OnDemandVacationDaysUsed,
            CircumstantialLeaveDaysUsed = user.CircumstantialLeaveDaysUsed,
            CarriedOverVacationDays = user.CarriedOverVacationDays,
            CarriedOverExpiryDate = user.CarriedOverExpiryDate,
            TotalAvailableVacationDays = user.TotalAvailableVacationDays
        };
    }
}
