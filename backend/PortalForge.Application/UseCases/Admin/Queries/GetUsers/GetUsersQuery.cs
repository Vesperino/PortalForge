using MediatR;

namespace PortalForge.Application.UseCases.Admin.Queries.GetUsers;

public class GetUsersQuery : IRequest<GetUsersResult>
{
    public string? SearchTerm { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

