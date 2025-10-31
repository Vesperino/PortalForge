using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.Exceptions;

namespace PortalForge.Application.UseCases.Departments.Commands.UpdateDepartment;

/// <summary>
/// Handler for updating an existing department.
/// </summary>
public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<UpdateDepartmentCommandHandler> _logger;

    public UpdateDepartmentCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<UpdateDepartmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        UpdateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating department: {DepartmentId}", request.DepartmentId);

        // 1. Validate the command
        await _validatorService.ValidateAsync(request);

        // 2. Get existing department
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", request.DepartmentId);
            throw new NotFoundException($"Department with ID {request.DepartmentId} not found");
        }

        // 3. Update properties
        department.Name = request.Name;
        department.Description = request.Description ?? string.Empty;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.HeadOfDepartmentId = request.DepartmentHeadId;
        department.IsActive = request.IsActive;
        department.UpdatedAt = DateTime.UtcNow;

        // 4. Persist changes
        await _unitOfWork.DepartmentRepository.UpdateAsync(department);

        _logger.LogInformation("Department updated successfully: {DepartmentId} - {DepartmentName}",
            department.Id, department.Name);

        return Unit.Value;
    }
}
