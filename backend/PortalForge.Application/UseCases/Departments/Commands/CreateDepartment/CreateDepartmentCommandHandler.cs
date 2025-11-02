using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Domain.Entities;

namespace PortalForge.Application.UseCases.Departments.Commands.CreateDepartment;

/// <summary>
/// Handler for creating a new department.
/// </summary>
public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;
    private readonly ILogger<CreateDepartmentCommandHandler> _logger;

    public CreateDepartmentCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService,
        ILogger<CreateDepartmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating department: {DepartmentName}", request.Name);

        // 1. Validate the command
        await _validatorService.ValidateAsync(request);

        // 2. Create department entity
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            ParentDepartmentId = request.ParentDepartmentId,
            HeadOfDepartmentId = request.DepartmentHeadId,
            DirectorId = request.DepartmentDirectorId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        // 3. Persist to database
        var createdDepartment = await _unitOfWork.DepartmentRepository.CreateAsync(department);

        _logger.LogInformation("Department created successfully: {DepartmentId} - {DepartmentName}",
            createdDepartment.Id, createdDepartment.Name);

        return createdDepartment.Id;
    }
}

