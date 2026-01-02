using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetUsersResult> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting users with filters: SearchTerm={SearchTerm}, Department={Department}, Role={Role}",
            request.SearchTerm, request.Department, request.Role);

        var (users, totalCount) = await _unitOfWork.UserRepository.GetFilteredAsync(
            request.SearchTerm,
            request.Department,
            request.Position,
            request.Role,
            request.IsActive,
            request.PageNumber,
            request.PageSize,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var usersList = users.ToList();

        var userIds = usersList.Select(u => u.Id).ToList();
        var allUserRoleGroups = await _unitOfWork.UserRoleGroupRepository.GetByUserIdsAsync(userIds, cancellationToken);
        var userRoleGroupsLookup = allUserRoleGroups
            .GroupBy(urg => urg.UserId)
            .ToDictionary(g => g.Key, g => g.Select(urg => urg.RoleGroup.Name).ToList());

        var userDtos = usersList.Select(user => new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Department = user.Department,
            DepartmentId = user.DepartmentId,
            Position = user.Position,
            PositionId = user.PositionId,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            IsEmailVerified = user.IsEmailVerified,
            MustChangePassword = user.MustChangePassword,
            PhoneNumber = user.PhoneNumber,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            RoleGroups = userRoleGroupsLookup.TryGetValue(user.Id, out var roleGroups) ? roleGroups : new List<string>(),
            AnnualVacationDays = user.AnnualVacationDays ?? 26,
            VacationDaysUsed = user.VacationDaysUsed ?? 0,
            OnDemandVacationDaysUsed = user.OnDemandVacationDaysUsed ?? 0,
            CarriedOverVacationDays = user.CarriedOverVacationDays ?? 0
        }).ToList();

        _logger.LogInformation("Found {Count} users (total: {TotalCount})", userDtos.Count, totalCount);

        return new GetUsersResult
        {
            Users = userDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

