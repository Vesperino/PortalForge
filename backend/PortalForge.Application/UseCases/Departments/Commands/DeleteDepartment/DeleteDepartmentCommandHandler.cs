using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Departments.Commands.DeleteDepartment;

/// <summary>
/// Handler for soft deleting a department.
/// </summary>
public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDepartmentCommandHandler> _logger;

    public DeleteDepartmentCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteDepartmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Soft deleting department: {DepartmentId}", request.DepartmentId);

        // 1. Get existing department
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", request.DepartmentId);
            throw new NotFoundException($"Department with ID {request.DepartmentId} not found");
        }

        // 2. Soft delete by setting IsActive to false
        await _unitOfWork.DepartmentRepository.DeleteAsync(request.DepartmentId);

        _logger.LogInformation("Department soft deleted successfully: {DepartmentId} - {DepartmentName}",
            department.Id, department.Name);

        return MediatR.Unit.Value;
    }
}
