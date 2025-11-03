using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;
using PortalForge.Application.Services;

namespace PortalForge.Application.UseCases.Departments.Queries.GetDepartmentVacationCalendar;

/// <summary>
/// Handler for GetDepartmentVacationCalendarQuery.
/// Retrieves vacation calendar for a department and its subdepartments.
/// </summary>
public class GetDepartmentVacationCalendarQueryHandler
    : IRequestHandler<GetDepartmentVacationCalendarQuery, List<VacationCalendarEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<GetDepartmentVacationCalendarQueryHandler> _logger;

    public GetDepartmentVacationCalendarQueryHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<GetDepartmentVacationCalendarQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<List<VacationCalendarEntryDto>> Handle(
        GetDepartmentVacationCalendarQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate
        await _validatorService.ValidateAsync(request);

        // 2. Check if department exists
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId)
            ?? throw new NotFoundException($"Department with ID {request.DepartmentId} not found");

        // 3. Get all vacations for department (including subdepartments)
        var vacations = await _unitOfWork.VacationScheduleRepository
            .GetTeamVacationsAsync(
                request.DepartmentId,
                request.FromDate,
                request.ToDate);

        // 4. Map to DTOs
        var result = vacations.Select(v => new VacationCalendarEntryDto
        {
            VacationId = v.Id,
            UserId = v.UserId,
            UserFullName = $"{v.User.FirstName} {v.User.LastName}",
            Position = v.User.PositionEntity?.Name ?? v.User.Position,
            StartDate = v.StartDate,
            EndDate = v.EndDate,
            DaysCount = v.DaysCount,
            Status = v.Status.ToString(),
            LeaveType = v.SourceRequest?.LeaveType?.ToString() ?? "Unknown"
        }).ToList();

        _logger.LogInformation(
            "Retrieved {Count} vacation entries for department {DepartmentId} from {FromDate} to {ToDate}",
            result.Count, request.DepartmentId, request.FromDate, request.ToDate);

        return result;
    }
}
