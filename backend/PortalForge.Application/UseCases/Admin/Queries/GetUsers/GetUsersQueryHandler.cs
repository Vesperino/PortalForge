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

        // Get all users with their role groups
        var query = _unitOfWork.UserRepository.GetAllAsync()
            .Result
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            query = query.Where(u => u.Department == request.Department);
        }

        if (!string.IsNullOrWhiteSpace(request.Position))
        {
            query = query.Where(u => u.Position == request.Position);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            query = query.Where(u => u.Role.ToString() == request.Role);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        // Get total count before pagination
        var totalCount = query.Count();

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "firstname" => request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "lastname" => request.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
            "department" => request.SortDescending ? query.OrderByDescending(u => u.Department) : query.OrderBy(u => u.Department),
            "position" => request.SortDescending ? query.OrderByDescending(u => u.Position) : query.OrderBy(u => u.Position),
            "createdat" => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        // Apply pagination
        var users = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Get role groups for each user
        var userDtos = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var userRoleGroups = await _unitOfWork.UserRoleGroupRepository.GetByUserIdAsync(user.Id);
            var roleGroupIds = userRoleGroups.Select(urg => urg.RoleGroupId).ToList();
            var roleGroups = (await _unitOfWork.RoleGroupRepository.GetAllAsync())
                .Where(rg => roleGroupIds.Contains(rg.Id))
                .Select(rg => rg.Name)
                .ToList();

            userDtos.Add(new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                DepartmentId = user.DepartmentId,
                Position = user.Position,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                MustChangePassword = user.MustChangePassword,
                PhoneNumber = user.PhoneNumber,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                RoleGroups = roleGroups
            });
        }

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

