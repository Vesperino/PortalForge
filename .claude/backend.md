# Backend Rules - .NET 8.0 + Clean Architecture + CQRS

## Clean Architecture Layers

The backend follows Clean Architecture with clear separation:

1. **Presentation Layer (PortalForge.Api)**
   - REST API controllers
   - Request/Response DTOs
   - Middleware (authentication, logging, error handling)
   - Application startup configuration

2. **Application Layer (PortalForge.Application)**
   - Use cases (commands and queries)
   - Business logic
   - Validation (FluentValidation)
   - Interfaces for infrastructure

3. **Domain Layer (PortalForge.Domain)**
   - Core business entities
   - Domain logic
   - Value objects
   - Domain events

4. **Infrastructure Layer (PortalForge.Infrastructure)**
   - Database access (EF Core)
   - Repository implementations
   - External services
   - Migrations

## CQRS Pattern with MediatR

All operations are commands or queries handled by MediatR:

**Command Example:**
```csharp
// Command definition
public class CreateEmployeeCommand : IRequest<int>, ITransactionalRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int DepartmentId { get; set; }
}

// Command handler
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUnifiedValidatorService _validatorService;

    public CreateEmployeeCommandHandler(
        IUnitOfWork unitOfWork,
        IUnifiedValidatorService validatorService)
    {
        _unitOfWork = unitOfWork;
        _validatorService = validatorService;
    }

    public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate request
        await _validatorService.ValidateAsync(request);

        // 2. Business logic
        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DepartmentId = request.DepartmentId
        };

        // 3. Persist changes
        var employeeId = await _unitOfWork.EmployeeRepository.CreateAsync(employee);

        return employeeId;
    }
}
```

**Query Example:**
```csharp
// Query definition
public class GetEmployeeByIdQuery : IRequest<EmployeeDto>
{
    public int EmployeeId { get; set; }
}

// Query handler
public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmployeeByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.EmployeeRepository.GetByIdAsync(request.EmployeeId)
            ?? throw new NotFoundException($"Employee with ID {request.EmployeeId} not found");

        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email
        };
    }
}
```

## Transaction Management

Commands that modify data should implement `ITransactionalRequest`. The `TransactionBehavior` automatically wraps these in database transactions:

```csharp
public class CreateEmployeeCommand : IRequest<int>, ITransactionalRequest
{
    // Command properties
}
```

## Repository Pattern with Unit of Work

All data access goes through repositories coordinated by UnitOfWork:

```csharp
// Repository interface
public interface IEmployeeRepository
{
    Task<Employee> GetByIdAsync(int id);
    Task<IEnumerable<Employee>> GetAllAsync();
    Task<int> CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
}

// Unit of Work interface
public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository EmployeeRepository { get; }
    IDepartmentRepository DepartmentRepository { get; }
    IEventRepository EventRepository { get; }
    INewsRepository NewsRepository { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

## Validation with FluentValidation

All commands and queries must have corresponding validators:

```csharp
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        // Field validations
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Department ID must be greater than 0")
            .MustAsync(DepartmentExists).WithMessage("Department does not exist");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingEmployee = await _unitOfWork.EmployeeRepository.GetByEmailAsync(email);
        return existingEmployee == null;
    }

    private async Task<bool> DepartmentExists(int departmentId, CancellationToken cancellationToken)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
        return department != null;
    }
}
```

## Exception Handling

Use custom exceptions for better error handling:

```csharp
// Custom exceptions
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationCustomException : Exception
{
    public List<string> Errors { get; }

    public ValidationCustomException(string message, List<string> errors)
        : base(message)
    {
        Errors = errors;
    }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

// Error handling in handler
public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
{
    await _validatorService.ValidateAsync(request);

    var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId)
        ?? throw new NotFoundException($"Department with ID {request.DepartmentId} not found");

    // ... rest of logic
}
```

## Logging with Serilog

Use structured logging throughout the application:

```csharp
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, int>
{
    private readonly ILogger<CreateEmployeeCommandHandler> _logger;

    public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating employee: {FirstName} {LastName}, Email: {Email}",
            request.FirstName, request.LastName, request.Email);

        try
        {
            // ... logic

            _logger.LogInformation("Employee created successfully with ID: {EmployeeId}", employeeId);
            return employeeId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee: {FirstName} {LastName}",
                request.FirstName, request.LastName);
            throw;
        }
    }
}
```

## Coding Standards

### General Principles

- **SOLID Principles**: Follow SOLID design principles
- **DRY**: Don't Repeat Yourself - extract common logic
- **YAGNI**: You Aren't Gonna Need It - avoid over-engineering
- **Separation of Concerns**: Each class should have a single responsibility

### Naming Conventions

- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Local variables, parameters, private fields
- **_camelCase**: Private fields with underscore prefix
- **UPPER_CASE**: Constants

```csharp
// Good examples
public class EmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private const int MAX_EMPLOYEES = 1000;

    public async Task<Employee> GetEmployeeAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        return employee;
    }
}
```

### Code Organization

1. **File structure in classes:**
   - Using statements
   - Namespace
   - Class declaration
   - Constants
   - Private fields
   - Constructor(s)
   - Public properties
   - Public methods
   - Private methods

2. **UseCases folder structure:**
   ```
   UseCases/
   └── Employees/
       ├── Commands/
       │   └── CreateEmployee/
       │       ├── CreateEmployeeCommand.cs
       │       ├── CreateEmployeeCommandHandler.cs
       │       └── Validation/
       │           └── CreateEmployeeCommandValidator.cs
       └── Queries/
           └── GetEmployeeById/
               ├── GetEmployeeByIdQuery.cs
               ├── GetEmployeeByIdQueryHandler.cs
               └── Validation/
                   └── GetEmployeeByIdQueryValidator.cs
   ```

### Error Handling Best Practices

- Handle errors and edge cases at the beginning of methods
- Use early returns for error conditions
- Place the happy path last in the method
- Avoid unnecessary else statements (use if-return pattern)
- Use guard clauses for preconditions
- Implement proper error logging
- Return meaningful error messages

```csharp
public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
{
    // 1. Guard clauses first
    if (request == null)
        throw new ArgumentNullException(nameof(request));

    // 2. Validation
    await _validatorService.ValidateAsync(request);

    // 3. Check preconditions with early returns
    var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(request.DepartmentId);
    if (department == null)
        throw new NotFoundException($"Department with ID {request.DepartmentId} not found");

    if (!department.IsActive)
        throw new ValidationCustomException("Cannot assign employee to inactive department",
            new List<string> { "Department is not active" });

    // 4. Happy path at the end
    var employee = new Employee
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        DepartmentId = request.DepartmentId
    };

    var employeeId = await _unitOfWork.EmployeeRepository.CreateAsync(employee);
    return employeeId;
}
```

### Async/Await Guidelines

- Always use async/await for I/O operations
- Use `ConfigureAwait(false)` for library code
- Don't use `async void` except for event handlers
- Name async methods with `Async` suffix
- Always return `Task` or `Task<T>` from async methods

```csharp
// Good
public async Task<Employee> GetEmployeeAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// Bad
public Employee GetEmployee(int id)
{
    return _repository.GetByIdAsync(id).Result; // Blocking!
}
```

### Entity Framework Core Guidelines

- Use async methods for all database operations
- Use `AsNoTracking()` for read-only queries
- Include related entities explicitly
- Use projections (Select) when you don't need full entities
- Configure relationships in `OnModelCreating`

```csharp
// Good - projection with AsNoTracking
public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync()
{
    return await _context.Employees
        .AsNoTracking()
        .Select(e => new EmployeeDto
        {
            EmployeeId = e.EmployeeId,
            FullName = $"{e.FirstName} {e.LastName}",
            Email = e.Email
        })
        .ToListAsync();
}

// Good - include related entities
public async Task<Employee> GetEmployeeWithDepartmentAsync(int id)
{
    return await _context.Employees
        .Include(e => e.Department)
        .FirstOrDefaultAsync(e => e.EmployeeId == id);
}
```

### XML Documentation

Write XML documentation for all public APIs:

```csharp
/// <summary>
/// Creates a new employee in the system.
/// </summary>
/// <param name="request">The employee creation request containing employee details.</param>
/// <param name="cancellationToken">Cancellation token for the async operation.</param>
/// <returns>The ID of the newly created employee.</returns>
/// <exception cref="ValidationCustomException">Thrown when validation fails.</exception>
/// <exception cref="NotFoundException">Thrown when referenced department doesn't exist.</exception>
public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
{
    // Implementation
}
```

---

**Auto-attach**: `**/*.cs` in backend projects
