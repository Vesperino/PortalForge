# PortalForge - AI Rules and Project Context

## Important Instructions
- NEVER add comments like "Generated with Claude Code" or similar attribution comments to any files
- NEVER add Co-Authored-By headers or similar metadata indicating AI assistance
- Focus on writing clean, maintainable, and well-documented code
- Always prioritize code quality and best practices over speed

## Project Overview

**PortalForge** is an internal intranet portal for organizations with 200+ employees. It's a centralized communication platform that solves documentation chaos and lack of information centralization through organizational structure management, company events calendar, and internal communication.

**Current Phase**: MVP Development - Phase 1 (Foundation)
**Timeline**: 8 weeks total
**Repository**: https://github.com/Vesperino/PortalForge.git

## Tech Stack

### Backend
- **Framework**: .NET 8.0 (LTS until November 2026)
- **Architecture**: Clean Architecture + CQRS (MediatR)
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL via Supabase
- **Authentication**: Supabase Auth
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Testing**: xUnit + FluentAssertions + Moq

### Frontend
- **Framework**: Nuxt 3 (Vue 3 with Composition API)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State Management**: Pinia
- **Testing**: Vitest (unit) + Playwright (E2E)
- **Utils**: VueUse

### Infrastructure
- **VCS**: Git + GitHub
- **CI/CD**: GitHub Actions
- **Containerization**: Docker
- **Database & Auth**: Supabase
- **Hosting**: VPS (to be configured)

## Project Structure (Monorepo)

```
PortalForge/
├── .ai/                              # AI Documentation & Context
│   ├── prd.md                       # Product Requirements Document
│   ├── tech-stack.md                # Tech Stack Analysis
│   ├── backend/                     # Backend documentation
│   └── frontend/                    # Frontend documentation
├── .claude/                          # Claude Code configuration
│   └── CLAUDE.md                    # This file
├── backend/
│   ├── PortalForge.Api/            # Presentation Layer
│   │   ├── Controllers/            # REST API controllers
│   │   ├── Middleware/             # Custom middleware
│   │   ├── Dtos/                   # Request/Response DTOs
│   │   └── Program.cs              # Application entry point
│   ├── PortalForge.Application/    # Application Layer
│   │   ├── Common/                 # Shared components
│   │   │   └── Behaviors/          # MediatR pipeline behaviors
│   │   ├── DTOs/                   # Application DTOs
│   │   ├── Exceptions/             # Custom exceptions
│   │   ├── Extensions/             # Extension methods
│   │   ├── Interfaces/             # Application interfaces
│   │   │   └── Persistence/        # Repository interfaces
│   │   └── UseCases/               # CQRS Commands and Queries
│   │       ├── Employees/
│   │       │   ├── Commands/
│   │       │   │   └── CreateEmployee/
│   │       │   │       ├── CreateEmployeeCommand.cs
│   │       │   │       ├── CreateEmployeeCommandHandler.cs
│   │       │   │       └── Validation/
│   │       │   │           └── CreateEmployeeCommandValidator.cs
│   │       │   └── Queries/
│   │       ├── Events/
│   │       ├── News/
│   │       └── Users/
│   ├── PortalForge.Domain/         # Domain Layer
│   │   ├── Entities/               # Domain entities
│   │   ├── Enums/                  # Domain enumerations
│   │   └── ValueObjects/           # Value objects
│   └── PortalForge.Infrastructure/ # Infrastructure Layer
│       ├── Configuration/          # Configuration providers
│       ├── Extensions/             # DI extensions
│       ├── Persistence/            # Data access
│       │   ├── ConnectionFactory/  # DB connection factory
│       │   ├── Migrations/         # EF Core migrations
│       │   └── Repositories/       # Repository implementations
│       └── Services/               # Infrastructure services
├── frontend/                        # Nuxt 3 Application
│   ├── components/                  # Vue components
│   ├── composables/                 # Composable functions
│   ├── layouts/                     # Layout components
│   ├── pages/                       # Page components (auto-routing)
│   ├── stores/                      # Pinia stores
│   ├── types/                       # TypeScript types
│   ├── utils/                       # Utility functions
│   ├── nuxt.config.ts              # Nuxt configuration
│   └── package.json                 # NPM dependencies
├── .gitignore
├── CLAUDE.md                        # Root project context
├── PortalForge.sln                  # Visual Studio Solution
└── README.md                        # Project README
```

## Development Commands

### Backend
```bash
# Navigate to backend API
cd backend/PortalForge.Api

# Restore dependencies
dotnet restore

# Run development server
dotnet run

# Run with hot reload
dotnet watch run

# Run tests
dotnet test

# Build solution
dotnet build

# EF Core migrations
dotnet ef migrations add <MigrationName> --project ../PortalForge.Infrastructure
dotnet ef database update --project ../PortalForge.Infrastructure
```

### Frontend
```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Development server (http://localhost:3000)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run unit tests
npm run test

# Run E2E tests
npm run test:e2e

# Lint and format
npm run lint
npm run format
```

## Backend Architecture & Patterns

### Clean Architecture Layers

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

### CQRS Pattern with MediatR

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

### Transaction Management

Commands that modify data should implement `ITransactionalRequest`. The `TransactionBehavior` automatically wraps these in database transactions:

```csharp
public class CreateEmployeeCommand : IRequest<int>, ITransactionalRequest
{
    // Command properties
}
```

### Repository Pattern with Unit of Work

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

### Validation with FluentValidation

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

### Exception Handling

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

### Logging with Serilog

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

## Backend Coding Standards

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

## Testing Standards

### Testing Framework

- **xUnit**: Main testing framework
- **FluentAssertions**: For readable assertions
- **Moq**: For mocking dependencies

### Test Organization

```
PortalForge.Tests/
├── Unit/
│   ├── Application/
│   │   └── UseCases/
│   │       └── Employees/
│   │           └── CreateEmployeeCommandHandlerTests.cs
│   └── Domain/
│       └── Entities/
│           └── EmployeeTests.cs
└── Integration/
    └── Api/
        └── EmployeesControllerTests.cs
```

### Test Naming Convention

Use descriptive names: `MethodName_StateUnderTest_ExpectedBehavior`

```csharp
public class CreateEmployeeCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesEmployee()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var validatorMock = new Mock<IUnifiedValidatorService>();

        var command = new CreateEmployeeCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            DepartmentId = 1
        };

        // Act
        var handler = new CreateEmployeeCommandHandler(
            unitOfWorkMock.Object,
            validatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        unitOfWorkMock.Verify(x => x.EmployeeRepository.CreateAsync(
            It.Is<Employee>(e =>
                e.FirstName == "John" &&
                e.LastName == "Doe" &&
                e.Email == "john.doe@example.com")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.DepartmentRepository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Department)null);

        var command = new CreateEmployeeCommand { DepartmentId = 999 };

        // Act
        var handler = new CreateEmployeeCommandHandler(
            unitOfWorkMock.Object,
            Mock.Of<IUnifiedValidatorService>());

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Department*not found*");
    }
}
```

### AAA Pattern

Always follow Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange - Set up test data and mocks
    var mock = new Mock<IRepository>();
    var service = new Service(mock.Object);

    // Act - Execute the method being tested
    var result = await service.MethodAsync();

    // Assert - Verify the results
    result.Should().NotBeNull();
    result.Should().Be(expectedValue);
}
```

## Frontend Architecture & Patterns

### Vue 3 Composition API

Use `<script setup>` syntax for all components:

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { Employee } from '~/types'

// Props
interface Props {
  employeeId: number
}
const props = defineProps<Props>()

// Emits
interface Emits {
  (e: 'update', employee: Employee): void
  (e: 'delete', id: number): void
}
const emit = defineEmits<Emits>()

// State
const employee = ref<Employee | null>(null)
const isLoading = ref(false)
const error = ref<string | null>(null)

// Computed
const fullName = computed(() =>
  employee.value ? `${employee.value.firstName} ${employee.value.lastName}` : ''
)

// Methods
async function loadEmployee() {
  isLoading.value = true
  error.value = null

  try {
    const data = await $fetch<Employee>(`/api/employees/${props.employeeId}`)
    employee.value = data
  } catch (err) {
    error.value = 'Failed to load employee'
    console.error(err)
  } finally {
    isLoading.value = false
  }
}

// Lifecycle
onMounted(() => {
  loadEmployee()
})
</script>

<template>
  <div class="employee-card">
    <div v-if="isLoading" class="loading">Loading...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <div v-else-if="employee" class="employee-details">
      <h2>{{ fullName }}</h2>
      <p>{{ employee.email }}</p>
      <button @click="emit('update', employee)">Edit</button>
      <button @click="emit('delete', employee.id)">Delete</button>
    </div>
  </div>
</template>
```

### TypeScript Standards

- Always define types and interfaces
- Use `type` for unions/intersections, `interface` for objects
- Avoid `any` - use `unknown` if type is truly unknown
- Use generics for reusable components

```typescript
// Types for API responses
export interface Employee {
  employeeId: number
  firstName: string
  lastName: string
  email: string
  departmentId: number
  department?: Department
}

export interface Department {
  departmentId: number
  name: string
  description: string
}

// API response wrapper
export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
  errors?: string[]
}

// Union types
export type EmployeeStatus = 'active' | 'inactive' | 'on-leave'

// Utility types
export type CreateEmployeeDto = Omit<Employee, 'employeeId' | 'department'>
export type UpdateEmployeeDto = Partial<CreateEmployeeDto>
```

### Composables

Create reusable composables for common logic:

```typescript
// composables/useEmployees.ts
export function useEmployees() {
  const employees = ref<Employee[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchEmployees() {
    isLoading.value = true
    error.value = null

    try {
      const data = await $fetch<ApiResponse<Employee[]>>('/api/employees')
      employees.value = data.data
    } catch (err) {
      error.value = 'Failed to fetch employees'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function createEmployee(dto: CreateEmployeeDto) {
    const data = await $fetch<ApiResponse<Employee>>('/api/employees', {
      method: 'POST',
      body: dto
    })
    employees.value.push(data.data)
    return data.data
  }

  return {
    employees: readonly(employees),
    isLoading: readonly(isLoading),
    error: readonly(error),
    fetchEmployees,
    createEmployee
  }
}
```

### Pinia Stores

Use Pinia for global state management:

```typescript
// stores/auth.ts
export const useAuthStore = defineStore('auth', () => {
  // State
  const user = ref<User | null>(null)
  const isAuthenticated = computed(() => user.value !== null)

  // Actions
  async function login(email: string, password: string) {
    const { data } = await $fetch<ApiResponse<User>>('/api/auth/login', {
      method: 'POST',
      body: { email, password }
    })
    user.value = data
  }

  function logout() {
    user.value = null
  }

  return {
    user: readonly(user),
    isAuthenticated,
    login,
    logout
  }
})
```

### Tailwind CSS Best Practices

- Use utility classes directly in templates
- Group related utilities logically
- Use responsive prefixes (sm:, md:, lg:, xl:)
- Leverage dark mode with `dark:` prefix
- Use arbitrary values `[]` sparingly

```vue
<template>
  <div class="flex flex-col gap-4 p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md">
    <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
      {{ title }}
    </h2>

    <p class="text-sm text-gray-600 dark:text-gray-300 leading-relaxed">
      {{ description }}
    </p>

    <div class="flex gap-2 justify-end">
      <button class="px-4 py-2 bg-blue-500 hover:bg-blue-600 text-white rounded transition">
        Save
      </button>
      <button class="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded transition">
        Cancel
      </button>
    </div>
  </div>
</template>
```

### Accessibility Guidelines

- Use semantic HTML elements
- Implement proper ARIA labels and roles
- Ensure keyboard navigation works
- Maintain proper heading hierarchy (h1 → h2 → h3)
- Implement focus states for interactive elements

```vue
<template>
  <nav aria-label="Main navigation">
    <ul role="list" class="flex gap-4">
      <li>
        <a href="/employees" aria-current="page" class="focus:ring-2 focus:ring-blue-500">
          Employees
        </a>
      </li>
      <li>
        <a href="/departments" class="focus:ring-2 focus:ring-blue-500">
          Departments
        </a>
      </li>
    </ul>
  </nav>

  <button
    aria-label="Close dialog"
    @click="closeDialog"
    class="focus:ring-2 focus:ring-blue-500"
  >
    <span aria-hidden="true">&times;</span>
  </button>
</template>
```

## Git Workflow & Commit Standards

### Branch Naming

- `feature/feature-name` - New features
- `fix/bug-name` - Bug fixes
- `refactor/description` - Code refactoring
- `docs/description` - Documentation updates

### Commit Message Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add employee creation endpoint
fix: resolve department validation issue
docs: update API documentation
refactor: extract validation logic to separate service
test: add tests for employee repository
chore: update dependencies
style: format code with prettier
```

### Commit Message Structure

```
<type>: <short description>

<optional detailed description>

<optional footer>
```

Examples:
```
feat: implement employee CSV import

Add functionality to import employees from CSV files.
Includes validation and error reporting.

Closes #123
```

## Do NOT

- ❌ **Do NOT** edit `appsettings.json` directly - use `appsettings.Development.json`
- ❌ **Do NOT** commit secrets, API keys, or passwords
- ❌ **Do NOT** commit `node_modules/`, `bin/`, `obj/` folders
- ❌ **Do NOT** force push to `main` branch
- ❌ **Do NOT** skip migrations - always generate EF migrations
- ❌ **Do NOT** use `any` type in TypeScript
- ❌ **Do NOT** bypass authentication/authorization checks
- ❌ **Do NOT** ignore TypeScript/ESLint errors
- ❌ **Do NOT** use `async void` (except event handlers)
- ❌ **Do NOT** block async code with `.Result` or `.Wait()`
- ❌ **Do NOT** create commands/queries without validators
- ❌ **Do NOT** access database directly from controllers
- ❌ **Do NOT** put business logic in controllers

## MVP Scope Reminders

### ✅ IN SCOPE (MVP)
- User authentication (Supabase Auth)
- Role-based access control (Admin, Manager, HR, Marketing, Employee)
- Organizational structure management (hierarchical tree)
- Calendar of company events
- News system
- User activity monitoring
- CSV/Excel import of users
- PDF/Excel export of org structure

### ❌ OUT OF SCOPE (Future)
- Request system with workflows
- Document management and versioning
- Active Directory/LDAP integration
- Dedicated admin panel
- Email/Push notifications
- Full-text search
- Internal chat/messenger
- External API integrations

## Key Business Rules

1. **Every employee must have**: First name, Last name, Email, Department, Position, Supervisor
2. **Only Admin/HR** can import users via CSV/Excel
3. **Only Admin/HR/Marketing** can create news and events
4. **Managers** can only edit their department structure
5. **Events** are archived after 1 year
6. **Sessions** expire after 8 hours of inactivity
7. **Passwords** must be hashed with bcrypt
8. **API responses** must be < 500ms for 95% of requests

## Supabase Configuration

**Project**: https://mqowlgphivdosieakzjb.supabase.co

**Environment Variables:**
- Frontend: `frontend/.env` (gitignored)
  - `NUXT_PUBLIC_SUPABASE_URL`
  - `NUXT_PUBLIC_SUPABASE_KEY` (anon key)
- Backend: `backend/PortalForge.Api/.env` (gitignored)
  - `SUPABASE_URL`
  - `SUPABASE_SERVICE_ROLE_KEY` (secret!)
  - `CONNECTION_STRING`

**⚠️ SECURITY**: Never commit `.env` files! Always use `.env.example` templates.

## Support & References

- **Documentation**: [.ai/prd.md](.ai/prd.md), [.ai/tech-stack.md](.ai/tech-stack.md)
- **.NET Docs**: https://learn.microsoft.com/en-us/dotnet/
- **Nuxt Docs**: https://nuxt.com/docs
- **Supabase Docs**: https://supabase.com/docs
- **Tailwind Docs**: https://tailwindcss.com/docs
- **MediatR Docs**: https://github.com/jbogard/MediatR/wiki
- **FluentValidation Docs**: https://docs.fluentvalidation.net/

---

**Last Updated**: 2025-10-11
**Version**: 1.0.0
