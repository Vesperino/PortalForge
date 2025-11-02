using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.SeedEmployees;

/// <summary>
/// Command to seed 40 sample employees with avatars.
/// </summary>
public class SeedEmployeesCommand : IRequest<SeedEmployeesResult>, ITransactionalRequest
{
}

public class SeedEmployeesResult
{
    public int EmployeesCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
