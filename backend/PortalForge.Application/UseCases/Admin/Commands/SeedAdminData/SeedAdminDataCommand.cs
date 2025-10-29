using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.Admin.Commands.SeedAdminData;

public class SeedAdminDataCommand : IRequest<SeedAdminDataResult>, ITransactionalRequest
{
}

public class SeedAdminDataResult
{
    public int PermissionsCreated { get; set; }
    public int RoleGroupsCreated { get; set; }
    public bool AdminUserCreated { get; set; }
    public string Message { get; set; } = string.Empty;
}

