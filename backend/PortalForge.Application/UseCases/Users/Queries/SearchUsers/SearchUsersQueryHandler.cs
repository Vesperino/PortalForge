using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;

namespace PortalForge.Application.UseCases.Users.Queries.SearchUsers;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserSearchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SearchUsersQueryHandler> _logger;

    public SearchUsersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<SearchUsersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<UserSearchDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching users with query: {Query}", request.Query);

        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
        {
            return new List<UserSearchDto>();
        }

        // Use server-side filtering instead of loading all users into memory
        var users = await _unitOfWork.UserRepository.SearchAsync(
            request.Query,
            request.OnlyActive,
            request.DepartmentId,
            request.Limit,
            cancellationToken);

        var result = users.Select(u => new UserSearchDto
        {
            Id = u.Id.ToString(),
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Position = u.Position,
            Department = u.Department,
            DepartmentId = u.DepartmentId?.ToString(),
            ProfilePhotoUrl = u.ProfilePhotoUrl
        }).ToList();

        _logger.LogInformation("Found {Count} users matching query: {Query}", result.Count, request.Query);

        return result;
    }
}
