using MediatR;
using PortalForge.Application.Common.Interfaces;

namespace PortalForge.Application.UseCases.InternalServices.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
}
