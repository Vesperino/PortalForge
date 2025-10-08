# Backend Documentation

## Overview
PortalForge Backend - .NET 8.0 Web API with Clean Architecture and CQRS

## Architecture

### Clean Architecture Layers

```
PortalForge.Api/           # Presentation Layer
├── Controllers/           # API Controllers
├── Middleware/           # Custom middleware
└── Program.cs            # Application entry point

PortalForge.Application/  # Application Layer (to be created)
├── Commands/             # CQRS Commands
├── Queries/              # CQRS Queries
├── DTOs/                 # Data Transfer Objects
├── Validators/           # FluentValidation validators
└── Mappings/             # AutoMapper profiles

PortalForge.Domain/       # Domain Layer (to be created)
├── Entities/             # Domain entities
├── ValueObjects/         # Value objects
├── Enums/                # Enumerations
└── Interfaces/           # Repository interfaces

PortalForge.Infrastructure/ # Infrastructure Layer (to be created)
├── Data/                 # EF Core DbContext
├── Repositories/         # Repository implementations
├── Auth/                 # Supabase Auth integration
└── Services/             # External services
```

## Current Status

### ✅ Completed
- Basic .NET 8.0 Web API project structure
- Solution file configured for monorepo

### 🚧 In Progress
- Setting up Clean Architecture structure
- Configuring Supabase connection

### ⏳ Planned
- Domain models (User, Employee, Department, etc.)
- CQRS implementation with MediatR
- Authentication with Supabase
- Repository pattern implementation
- API endpoints for MVP features

## Key Technologies

- **.NET 8.0**: LTS framework
- **MediatR**: CQRS pattern implementation
- **Entity Framework Core**: ORM
- **FluentValidation**: Input validation
- **AutoMapper**: Object mapping
- **Serilog**: Structured logging
- **Npgsql**: PostgreSQL driver
- **xUnit**: Testing framework

## Database Schema (Planned)

### Core Tables
- `Users` - Authentication and user data
- `Employees` - Employee details and profile
- `Departments` - Department structure
- `Positions` - Job positions/titles
- `OrganizationStructure` - Hierarchical relationships
- `Events` - Company events
- `News` - News articles
- `EventTags` - Tags for events
- `EventDepartments` - Event-Department mapping
- `AuditLogs` - Activity tracking

## API Endpoints (Planned)

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh token
- `GET /api/auth/me` - Current user info

### Users & Employees
- `GET /api/employees` - List employees
- `GET /api/employees/{id}` - Get employee details
- `POST /api/employees` - Create employee
- `PUT /api/employees/{id}` - Update employee
- `DELETE /api/employees/{id}` - Delete employee
- `POST /api/employees/import` - Import from CSV/Excel

### Organizational Structure
- `GET /api/organization/tree` - Get org tree
- `GET /api/organization/department/{id}` - Get department tree
- `GET /api/organization/export/pdf` - Export to PDF
- `GET /api/organization/export/excel` - Export to Excel
- `GET /api/organization/history` - Get change history

### Events & Calendar
- `GET /api/events` - List events
- `GET /api/events/{id}` - Get event details
- `POST /api/events` - Create event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Delete event

### News
- `GET /api/news` - List news
- `GET /api/news/{id}` - Get news details
- `POST /api/news` - Create news
- `PUT /api/news/{id}` - Update news
- `DELETE /api/news/{id}` - Delete news

### Reports
- `GET /api/reports/active-users` - Active users report
- `GET /api/reports/activity-log` - Activity log

## Development Guidelines

### Creating a New Feature

1. **Define Domain Entity** (`Domain/Entities/`)
2. **Create Command/Query** (`Application/Commands|Queries/`)
3. **Add Validator** (`Application/Validators/`)
4. **Implement Handler** (`Application/Commands|Queries/`)
5. **Create Controller** (`Api/Controllers/`)
6. **Write Tests** (`Tests/`)

### Example: Creating Employee

```csharp
// 1. Domain Entity
public class Employee : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; }
}

// 2. Command
public class CreateEmployeeCommand : IRequest<Result<EmployeeDto>>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

// 3. Validator
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
    }
}

// 4. Handler
public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
{
    // Implementation
}

// 5. Controller
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

## Testing Strategy

### Unit Tests
- Test domain logic
- Test validators
- Test command/query handlers
- Mock external dependencies

### Integration Tests
- Test API endpoints
- Test database operations
- Use in-memory database or test containers

### Test Structure
```
PortalForge.Tests/
├── Unit/
│   ├── Domain/
│   ├── Application/
│   └── Validators/
└── Integration/
    └── Controllers/
```

## Error Handling

Use Result pattern for operation outcomes:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> Errors { get; set; }
}
```

## Logging

Use Serilog with structured logging:

```csharp
_logger.LogInformation("Employee {EmployeeId} created by {UserId}", employeeId, userId);
```

## Security Considerations

- ✅ Use Supabase Auth for authentication
- ✅ Implement role-based authorization with policies
- ✅ Validate all inputs with FluentValidation
- ✅ Use HTTPS only
- ✅ Implement CORS properly
- ✅ Hash passwords with bcrypt
- ✅ Sanitize all user inputs
- ✅ Implement rate limiting
- ✅ Log security events

## Performance Optimization

- Use async/await for all I/O operations
- Implement caching for frequently accessed data
- Use pagination for large datasets
- Optimize database queries (indexes, includes)
- Use response compression
- Implement CDN for static assets

---

**Last Updated**: 2025-10-08
