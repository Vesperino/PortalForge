# Backend Refactoring Plan - Clean Architecture Compliance

**Project**: PortalForge
**Plan Date**: 2025-11-02
**Estimated Effort**: 40-60 developer hours
**Priority**: 🔴 CRITICAL
**Target Completion**: Sprint +2

---

## Overview

This plan addresses 47 violations of Clean Architecture and CQRS patterns identified in the code quality audit. Violations are organized by priority and estimated effort.

**Reference Document**: `BACKEND_CODE_QUALITY_AUDIT.md`

---

## Phase 1: CRITICAL FIXES (Week 1)

### Estimated Effort: 16-20 hours

These violations pose immediate risks to code maintainability, testability, and security.

---

### 1.1 Refactor LocationsController ⏱️ 4-5 hours

**File**: `backend/PortalForge.Api/Controllers/LocationsController.cs`

**Current Issues**:
- Direct DbContext access
- Business logic for geocoding
- HTTP client management in controller
- Entity creation

**Refactoring Steps**:

#### Step 1: Create Query for Getting Cached Locations
```bash
# Create folder structure
mkdir -p backend/PortalForge.Application/UseCases/Locations/Queries/GetCachedLocations

# Create files:
# - GetCachedLocationsQuery.cs
# - GetCachedLocationsQueryHandler.cs
# - GetCachedLocationsResult.cs
# - CachedLocationDto.cs (move from controller)
```

**GetCachedLocationsQuery.cs**:
```csharp
public class GetCachedLocationsQuery : IRequest<GetCachedLocationsResult>
{
    // No parameters - returns all
}
```

**GetCachedLocationsQueryHandler.cs**:
```csharp
public class GetCachedLocationsQueryHandler : IRequestHandler<GetCachedLocationsQuery, GetCachedLocationsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<GetCachedLocationsResult> Handle(...)
    {
        var locations = await _unitOfWork.CachedLocationRepository.GetAllAsync();

        return new GetCachedLocationsResult
        {
            Locations = locations.Select(l => new CachedLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                // ... mapping
            }).ToList()
        };
    }
}
```

#### Step 2: Create Command for Geocoding Address
```bash
mkdir -p backend/PortalForge.Application/UseCases/Locations/Commands/GeocodeAddress
mkdir -p backend/PortalForge.Application/UseCases/Locations/Commands/GeocodeAddress/Validation
```

**GeocodeAddressCommand.cs**:
```csharp
public class GeocodeAddressCommand : IRequest<GeocodeAddressResult>
{
    public string Address { get; set; } = string.Empty;
}
```

**GeocodeAddressCommandValidator.cs**:
```csharp
public class GeocodeAddressCommandValidator : AbstractValidator<GeocodeAddressCommand>
{
    public GeocodeAddressCommandValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MinimumLength(3).WithMessage("Address must be at least 3 characters");
    }
}
```

**GeocodeAddressCommandHandler.cs**:
```csharp
public class GeocodeAddressCommandHandler : IRequestHandler<GeocodeAddressCommand, GeocodeAddressResult>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeocodeAddressCommandHandler> _logger;

    public async Task<GeocodeAddressResult> Handle(...)
    {
        // Move HTTP client logic here
        // Move parsing logic here
        // Move fallback logic here
    }
}
```

#### Step 3: Create Commands for CRUD Operations
```bash
mkdir -p backend/PortalForge.Application/UseCases/Locations/Commands/CreateCachedLocation
mkdir -p backend/PortalForge.Application/UseCases/Locations/Commands/DeleteCachedLocation
```

#### Step 4: Update Controller
```csharp
[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("cached")]
    public async Task<IActionResult> GetCachedLocations()
    {
        var query = new GetCachedLocationsQuery();
        var result = await _mediator.Send(query);
        return Ok(result.Locations);
    }

    [HttpPost("geocode")]
    public async Task<IActionResult> GeocodeAddress([FromBody] GeocodeRequest request)
    {
        var command = new GeocodeAddressCommand { Address = request.Address };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ... other endpoints - all using MediatR
}
```

#### Step 5: Move DTOs to Separate Files
```bash
mkdir -p backend/PortalForge.Api/DTOs/Locations

# Create separate files:
# - GeocodeRequest.cs
# - GeocodeResult.cs
# - CreateCachedLocationRequest.cs
```

**Checklist**:
- [ ] Create GetCachedLocationsQuery + Handler
- [ ] Create GeocodeAddressCommand + Handler + Validator
- [ ] Create CreateCachedLocationCommand + Handler + Validator
- [ ] Create DeleteCachedLocationCommand + Handler
- [ ] Move all DTOs to separate files
- [ ] Update controller to use MediatR only
- [ ] Remove DbContext from controller
- [ ] Test all endpoints
- [ ] Delete old code

---

### 1.2 Refactor SystemSettingsController ⏱️ 3-4 hours

**File**: `backend/PortalForge.Api/Controllers/SystemSettingsController.cs`

**Refactoring Steps**:

#### Step 1: Create Queries
```bash
mkdir -p backend/PortalForge.Application/UseCases/SystemSettings/Queries/GetAllSettings
mkdir -p backend/PortalForge.Application/UseCases/SystemSettings/Queries/GetSettingByKey
```

#### Step 2: Create Commands
```bash
mkdir -p backend/PortalForge.Application/UseCases/SystemSettings/Commands/UpdateSettings
mkdir -p backend/PortalForge.Application/UseCases/SystemSettings/Commands/TestStorage
```

**UpdateSettingsCommand.cs**:
```csharp
public class UpdateSettingsCommand : IRequest<UpdateSettingsResult>, ITransactionalRequest
{
    public List<UpdateSettingDto> Settings { get; set; } = new();
    public Guid UpdatedBy { get; set; }
}
```

**UpdateSettingsCommandValidator.cs**:
```csharp
public class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator()
    {
        RuleFor(x => x.Settings)
            .NotEmpty().WithMessage("At least one setting is required");

        RuleForEach(x => x.Settings).ChildRules(setting =>
        {
            setting.RuleFor(s => s.Key)
                .NotEmpty().WithMessage("Setting key is required");
        });
    }
}
```

#### Step 3: Update Controller
```csharp
[HttpPut("bulk")]
public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
{
    var userId = GetUserId();
    var command = new UpdateSettingsCommand
    {
        Settings = request.Updates,
        UpdatedBy = userId
    };
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**Checklist**:
- [ ] Create GetAllSettingsQuery + Handler
- [ ] Create GetSettingByKeyQuery + Handler + Validator
- [ ] Create UpdateSettingsCommand + Handler + Validator
- [ ] Create TestStorageCommand + Handler
- [ ] Move DTOs to separate files
- [ ] Update controller
- [ ] Remove DbContext from controller
- [ ] Test all endpoints

---

### 1.3 Fix AuthController Data Access ⏱️ 2-3 hours

**File**: `backend/PortalForge.Api/Controllers/AuthController.cs`

**Current Issue**: Controller fetches user data after Login command

**Refactoring Steps**:

#### Step 1: Update LoginCommandResult
```csharp
public class LoginCommandResult
{
    public Guid UserId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;

    // Add complete user data
    public UserDto User { get; set; } = new();
}
```

#### Step 2: Update LoginCommandHandler
```csharp
public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
{
    // ... authentication logic ...

    // Fetch full user data in handler
    var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

    return new LoginCommandResult
    {
        UserId = userId,
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        User = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            // ... complete mapping
        }
    };
}
```

#### Step 3: Update Controller
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var command = new LoginCommand
    {
        Email = request.Email,
        Password = request.Password
    };

    var result = await _mediator.Send(command);

    // Just set cookie and return result - NO data access
    SetRefreshTokenCookie(result.RefreshToken);

    return Ok(new AuthResponseDto
    {
        User = result.User,  // Already complete from handler
        AccessToken = result.AccessToken
    });
}
```

**Checklist**:
- [ ] Update LoginCommandResult with UserDto
- [ ] Move user fetching to LoginCommandHandler
- [ ] Move DTO mapping to LoginCommandHandler
- [ ] Simplify controller to just call handler
- [ ] Test login flow
- [ ] Verify no database access in controller

---

### 1.4 Create Critical Missing Validators ⏱️ 4-5 hours

**Priority Commands Needing Validators**:

#### 1. CreateUserCommandValidator
```bash
mkdir -p backend/PortalForge.Application/UseCases/Admin/Commands/CreateUser/Validation
```

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(BeValidRole).WithMessage("Invalid role");
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(email);
        return existingUser == null;
    }

    private bool BeValidRole(string role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}
```

#### 2. UpdateUserCommandValidator
```bash
mkdir -p backend/PortalForge.Application/UseCases/Admin/Commands/UpdateUser/Validation
```

```csharp
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MustAsync(UserExists).WithMessage("User not found");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100);

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(BeValidRole).WithMessage("Invalid role");

        When(x => x.DepartmentId.HasValue, () =>
        {
            RuleFor(x => x.DepartmentId.Value)
                .MustAsync(DepartmentExists).WithMessage("Department not found");
        });

        When(x => x.PositionId.HasValue, () =>
        {
            RuleFor(x => x.PositionId.Value)
                .MustAsync(PositionExists).WithMessage("Position not found");
        });
    }

    private async Task<bool> UserExists(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        return user != null;
    }

    private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
    {
        var dept = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
        return dept != null;
    }

    private async Task<bool> PositionExists(Guid positionId, CancellationToken cancellationToken)
    {
        var position = await _unitOfWork.PositionRepository.GetByIdAsync(positionId);
        return position != null;
    }

    private bool BeValidRole(string role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}
```

#### 3. ChangePasswordCommandValidator
```bash
mkdir -p backend/PortalForge.Application/UseCases/Auth/Commands/ChangePassword/Validation
```

```csharp
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}
```

#### 4. SubmitRequestCommandValidator
```bash
mkdir -p backend/PortalForge.Application/UseCases/Requests/Commands/SubmitRequest/Validation
```

#### 5. CreateRequestTemplateCommandValidator
```bash
mkdir -p backend/PortalForge.Application/UseCases/RequestTemplates/Commands/CreateRequestTemplate/Validation
```

**Checklist**:
- [ ] Create CreateUserCommandValidator
- [ ] Create UpdateUserCommandValidator
- [ ] Create ChangePasswordCommandValidator
- [ ] Create SubmitRequestCommandValidator
- [ ] Create CreateRequestTemplateCommandValidator
- [ ] Update handlers to use IUnifiedValidatorService
- [ ] Test validation logic
- [ ] Ensure validators are registered in DI

---

### 1.5 Refactor StorageController ⏱️ 2-3 hours

**File**: `backend/PortalForge.Api/Controllers/StorageController.cs`

**Refactoring Steps**:

#### Step 1: Create Upload Command
```bash
mkdir -p backend/PortalForge.Application/UseCases/Storage/Commands/UploadNewsImage
mkdir -p backend/PortalForge.Application/UseCases/Storage/Commands/UploadNewsImage/Validation
```

**UploadNewsImageCommand.cs**:
```csharp
public class UploadNewsImageCommand : IRequest<UploadNewsImageResult>
{
    public IFormFile File { get; set; } = null!;
    public Guid UploadedBy { get; set; }
}
```

**UploadNewsImageCommandValidator.cs**:
```csharp
public class UploadNewsImageCommandValidator : AbstractValidator<UploadNewsImageCommand>
{
    private readonly ISystemSettingsService _systemSettings;

    public UploadNewsImageCommandValidator(ISystemSettingsService systemSettings)
    {
        _systemSettings = systemSettings;

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required")
            .Must(HaveValidSize).WithMessage("File size exceeds limit")
            .Must(HaveValidExtension).WithMessage("Invalid file type. Allowed: JPG, PNG, GIF, WebP");

        RuleFor(x => x.UploadedBy)
            .NotEmpty().WithMessage("User ID is required");
    }

    private bool HaveValidSize(IFormFile file)
    {
        var maxFileSizeMB = int.Parse(_systemSettings.GetValue("Storage:MaxFileSizeMB", "10"));
        var maxFileSize = maxFileSizeMB * 1024 * 1024;
        return file.Length <= maxFileSize;
    }

    private bool HaveValidExtension(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}
```

**UploadNewsImageCommandHandler.cs**:
```csharp
public class UploadNewsImageCommandHandler : IRequestHandler<UploadNewsImageCommand, UploadNewsImageResult>
{
    private readonly IStorageService _storageService;
    private readonly ILogger<UploadNewsImageCommandHandler> _logger;

    public async Task<UploadNewsImageResult> Handle(...)
    {
        // Move file upload logic here
        var fileName = await _storageService.UploadFileAsync(
            request.File,
            "news-images",
            request.UploadedBy
        );

        return new UploadNewsImageResult
        {
            FileName = fileName,
            Url = $"/storage/news-images/{fileName}"
        };
    }
}
```

#### Step 2: Update Controller
```csharp
[HttpPost("upload/news-images")]
public async Task<IActionResult> UploadNewsImage(IFormFile file)
{
    var userId = GetUserId();
    var command = new UploadNewsImageCommand
    {
        File = file,
        UploadedBy = userId
    };

    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**Checklist**:
- [ ] Create UploadNewsImageCommand + Handler + Validator
- [ ] Move file validation to validator
- [ ] Move upload logic to handler
- [ ] Create GetFileQuery + Handler
- [ ] Update controller
- [ ] Test file upload
- [ ] Test validation

---

## Phase 2: HIGH PRIORITY FIXES (Week 2)

### Estimated Effort: 12-16 hours

---

### 2.1 Split Controller DTOs into Separate Files ⏱️ 3-4 hours

**Affected Controllers**:
- UsersController (4 DTOs)
- LocationsController (6 DTOs)
- SystemSettingsController (5 DTOs)
- StorageController (1 DTO)
- RequestsController (2 DTOs)
- RoleGroupsController (2 DTOs)

**Directory Structure**:
```bash
mkdir -p backend/PortalForge.Api/DTOs/Requests
mkdir -p backend/PortalForge.Api/DTOs/Responses
```

**Steps**:

1. **Extract DTOs from UsersController**:
```bash
# Create files:
# backend/PortalForge.Api/DTOs/Requests/Users/BulkAssignDepartmentRequest.cs
# backend/PortalForge.Api/DTOs/Requests/Users/CreateUserRequest.cs
# backend/PortalForge.Api/DTOs/Requests/Users/UpdateUserRequest.cs
```

2. **Extract DTOs from LocationsController**:
```bash
# Create files:
# backend/PortalForge.Api/DTOs/Requests/Locations/GeocodeRequest.cs
# backend/PortalForge.Api/DTOs/Requests/Locations/CreateCachedLocationRequest.cs
# backend/PortalForge.Api/DTOs/Responses/Locations/CachedLocationDto.cs
# backend/PortalForge.Api/DTOs/Responses/Locations/GeocodeResult.cs
# backend/PortalForge.Api/DTOs/Responses/Locations/NominatimResult.cs
```

3. **Repeat for remaining controllers**

**Checklist**:
- [ ] Create DTOs folder structure
- [ ] Move all DTOs from UsersController
- [ ] Move all DTOs from LocationsController
- [ ] Move all DTOs from SystemSettingsController
- [ ] Move all DTOs from StorageController
- [ ] Move all DTOs from RequestsController
- [ ] Move all DTOs from RoleGroupsController
- [ ] Update controller imports
- [ ] Verify compilation
- [ ] Test all endpoints

---

### 2.2 Create Remaining High-Priority Validators ⏱️ 4-5 hours

**Commands Needing Validators**:

1. **ApproveRequestStepCommandValidator** ⏱️ 1 hour
2. **RejectRequestStepCommandValidator** ⏱️ 1 hour
3. **UpdateRequestTemplateCommandValidator** ⏱️ 1.5 hours
4. **CreateRoleGroupCommandValidator** ⏱️ 1 hour
5. **UpdateRoleGroupCommandValidator** ⏱️ 1 hour

**Template for Each Validator**:
```csharp
public class [CommandName]Validator : AbstractValidator<[CommandName]>
{
    private readonly IUnitOfWork _unitOfWork;

    public [CommandName]Validator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        // Add validation rules
        RuleFor(x => x.Property)
            .NotEmpty().WithMessage("Property is required");

        // Add async validation if needed
        RuleFor(x => x.Id)
            .MustAsync(EntityExists).WithMessage("Entity not found");
    }

    private async Task<bool> EntityExists(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository.GetByIdAsync(id);
        return entity != null;
    }
}
```

**Checklist**:
- [ ] Create ApproveRequestStepCommandValidator
- [ ] Create RejectRequestStepCommandValidator
- [ ] Create UpdateRequestTemplateCommandValidator
- [ ] Create CreateRoleGroupCommandValidator
- [ ] Create UpdateRoleGroupCommandValidator
- [ ] Register validators in DI
- [ ] Test validation logic
- [ ] Ensure handlers use IUnifiedValidatorService

---

### 2.3 Extract NewsController Enum Parsing ⏱️ 1-2 hours

**Current Issue**: Controller validates enum parsing

**Refactoring Steps**:

#### Step 1: Update CreateNewsCommandValidator
```csharp
public class CreateNewsCommandValidator : AbstractValidator<CreateNewsCommand>
{
    public CreateNewsCommandValidator()
    {
        // ... existing rules ...

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .Must(BeValidCategory).WithMessage("Invalid news category");
    }

    private bool BeValidCategory(string category)
    {
        return Enum.TryParse<NewsCategory>(category, true, out _);
    }
}
```

#### Step 2: Update NewsController
```csharp
[HttpPost]
public async Task<IActionResult> CreateNews([FromBody] CreateNewsRequest request)
{
    var userId = GetUserId();

    var command = new CreateNewsCommand
    {
        // ... map fields ...
        Category = request.Category,  // Validator will check this
        CreatedBy = userId
    };

    var result = await _mediator.Send(command);
    return Ok(result);
}
```

**Checklist**:
- [ ] Add category validation to CreateNewsCommandValidator
- [ ] Add category validation to UpdateNewsCommandValidator
- [ ] Remove enum parsing from NewsController
- [ ] Test news creation/update
- [ ] Verify validation errors return correctly

---

### 2.4 Create Base Controller for Common Operations ⏱️ 2 hours

**Purpose**: Centralize user ID extraction

**Implementation**:

```csharp
// backend/PortalForge.Api/Controllers/Base/BaseApiController.cs
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedException("User ID not found in claims");
        }

        return Guid.Parse(userIdClaim);
    }

    protected string GetUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new UnauthorizedException("Email not found in claims");
    }

    protected string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new UnauthorizedException("Role not found in claims");
    }
}
```

**Usage**:
```csharp
public class UsersController : BaseApiController  // Inherit from base
{
    // Remove GetUserId() method - use inherited version

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var userId = GetUserId();  // Use base class method
        // ...
    }
}
```

**Checklist**:
- [ ] Create BaseApiController
- [ ] Add GetUserId(), GetUserEmail(), GetUserRole() methods
- [ ] Update all controllers to inherit from BaseApiController
- [ ] Remove duplicate GetUserId() methods
- [ ] Test authentication flow
- [ ] Verify claim extraction works

---

## Phase 3: MEDIUM PRIORITY FIXES (Week 3)

### Estimated Effort: 10-14 hours

---

### 3.1 Split Application Layer DTOs ⏱️ 4-5 hours

**Files to Split**:

1. **DepartmentDto.cs** → Split into 4 files
2. **VacationCalendar.cs** → Split into 3 files
3. **GetRoleGroupsResult.cs** → Split into 3 files
4. **GetUsersResult.cs** → Split into 2 files (already has AdminUserDto, keep as-is)
5. **RequestTemplateDto.cs** → Split into 4 files
6. **RequestDto.cs** → Split into 2 files
7. **SeedAdminDataCommand.cs** → Extract DTO class
8. **SeedEmployeesCommand.cs** → Extract DTO class

**Example for DepartmentDto.cs**:

Current file has:
- DepartmentDto
- DepartmentTreeDto
- CreateDepartmentDto
- UpdateDepartmentDto

Split into:
```bash
backend/PortalForge.Application/DTOs/
  ├── DepartmentDto.cs
  ├── DepartmentTreeDto.cs
  ├── CreateDepartmentDto.cs
  └── UpdateDepartmentDto.cs
```

**Checklist**:
- [ ] Split DepartmentDto.cs
- [ ] Split VacationCalendar.cs
- [ ] Split GetRoleGroupsResult.cs (separate PermissionDto, RoleGroupDto)
- [ ] Split RequestTemplateDto.cs
- [ ] Split RequestDto.cs
- [ ] Extract DTOs from SeedAdminDataCommand.cs
- [ ] Extract DTOs from SeedEmployeesCommand.cs
- [ ] Update all imports
- [ ] Verify compilation
- [ ] Run tests

---

### 3.2 Create Remaining Validators ⏱️ 5-6 hours

**Commands Still Missing Validators**:

1. **VerifyEmailCommandValidator** ⏱️ 30 min
2. **MarkAsReadCommandValidator** ⏱️ 30 min
3. **DeletePositionCommandValidator** ⏱️ 30 min
4. **DeleteNewsCommandValidator** ⏱️ 30 min
5. **DeleteRequestTemplateCommandValidator** ⏱️ 30 min

**Low Priority Delete Command Validators** (can use simpler validation):
```csharp
public class Delete[Entity]CommandValidator : AbstractValidator<Delete[Entity]Command>
{
    private readonly IUnitOfWork _unitOfWork;

    public Delete[Entity]CommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required")
            .MustAsync(EntityExists).WithMessage("[Entity] not found");
    }

    private async Task<bool> EntityExists(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.[Entity]Repository.GetByIdAsync(id);
        return entity != null;
    }
}
```

**Checklist**:
- [ ] Create VerifyEmailCommandValidator
- [ ] Create MarkAsReadCommandValidator
- [ ] Create DeletePositionCommandValidator
- [ ] Create DeleteNewsCommandValidator
- [ ] Create DeleteRequestTemplateCommandValidator
- [ ] Create DeleteRoleGroupCommandValidator
- [ ] Create DeleteUserCommandValidator
- [ ] Register all validators in DI
- [ ] Test validation

---

### 3.3 Standardize Result DTOs ⏱️ 1-2 hours

**Issue**: Some commands return primitives instead of Result objects

**Examples of Primitives**:
```csharp
// BAD
public class CreateDepartmentCommand : IRequest<int>

// GOOD
public class CreateDepartmentCommand : IRequest<CreateDepartmentResult>

public class CreateDepartmentResult
{
    public int DepartmentId { get; set; }
    public string Message { get; set; } = "Department created successfully";
}
```

**Benefits of Result Objects**:
- Can add metadata (messages, warnings)
- Easier to extend in future
- More consistent API responses
- Better for error handling

**Checklist**:
- [ ] Identify commands returning primitives
- [ ] Create Result classes for each
- [ ] Update command definitions
- [ ] Update handlers to return Result objects
- [ ] Update controllers
- [ ] Test all affected endpoints

---

## Phase 4: LOW PRIORITY IMPROVEMENTS (Week 4)

### Estimated Effort: 2-4 hours

---

### 4.1 Delete WeatherForecastController ⏱️ 5 min

```bash
rm backend/PortalForge.Api/Controllers/WeatherForecastController.cs
rm backend/PortalForge.Domain/Entities/WeatherForecast.cs  # if exists
```

**Checklist**:
- [ ] Delete WeatherForecastController.cs
- [ ] Delete any related test files
- [ ] Verify compilation
- [ ] Commit changes

---

### 4.2 Add XML Documentation to Public APIs ⏱️ 2-3 hours

**Purpose**: Improve code documentation and enable XML doc generation for API

**Example**:
```csharp
/// <summary>
/// Creates a new department in the organizational structure.
/// </summary>
/// <param name="request">The department creation request containing department details.</param>
/// <param name="cancellationToken">Cancellation token for the async operation.</param>
/// <returns>The ID of the newly created department.</returns>
/// <exception cref="ValidationException">Thrown when validation fails.</exception>
/// <exception cref="NotFoundException">Thrown when parent department doesn't exist.</exception>
public async Task<CreateDepartmentResult> Handle(
    CreateDepartmentCommand request,
    CancellationToken cancellationToken)
{
    // Implementation
}
```

**Checklist**:
- [ ] Add XML docs to all command handlers
- [ ] Add XML docs to all query handlers
- [ ] Add XML docs to all validators
- [ ] Add XML docs to controller actions
- [ ] Enable XML doc generation in .csproj
- [ ] Generate API documentation

---

### 4.3 Review and Update .editorconfig ⏱️ 30 min

**Purpose**: Enforce coding standards automatically

**Recommended Settings**:
```ini
# One class per file
dotnet_diagnostic.CA1052.severity = warning

# Async method naming
dotnet_diagnostic.ASYNC0001.severity = warning

# Unused imports
dotnet_diagnostic.IDE0005.severity = warning
```

**Checklist**:
- [ ] Review current .editorconfig
- [ ] Add rules for one-class-per-file
- [ ] Add rules for async naming conventions
- [ ] Add rules for unused imports
- [ ] Run code cleanup
- [ ] Commit .editorconfig

---

## Testing Strategy

### Unit Tests

For each refactored component, create:

1. **Command Handler Tests**:
   - Happy path
   - Validation failures
   - Not found scenarios
   - Permission errors

2. **Validator Tests**:
   - Each validation rule
   - Async validation
   - Edge cases

3. **Controller Tests** (simplified after refactoring):
   - Request mapping
   - Response formatting
   - Error handling

### Integration Tests

1. **End-to-End Flow Tests**:
   - Create → Read → Update → Delete
   - Authentication flow
   - Authorization checks

2. **API Contract Tests**:
   - Request/response schemas
   - HTTP status codes
   - Error responses

---

## Success Criteria

### Phase 1 Completion ✅
- [ ] All CRITICAL controllers refactored
- [ ] All CRITICAL validators created
- [ ] No business logic in any controller
- [ ] All tests passing
- [ ] Code coverage > 70%

### Phase 2 Completion ✅
- [ ] All DTOs in separate files
- [ ] All HIGH priority validators created
- [ ] Base controller implemented
- [ ] All tests passing

### Phase 3 Completion ✅
- [ ] All Application DTOs in separate files
- [ ] All commands have validators
- [ ] Result objects standardized
- [ ] All tests passing

### Phase 4 Completion ✅
- [ ] Demo code removed
- [ ] XML documentation complete
- [ ] Code standards enforced
- [ ] Final code review passed

---

## Rollout Plan

### Week 1: CRITICAL Fixes
- Focus on LocationsController and SystemSettingsController
- Create critical validators
- Daily code reviews
- Continuous testing

### Week 2: HIGH Priority Fixes
- Split DTOs
- Create remaining high-priority validators
- Mid-sprint review and adjustment

### Week 3: MEDIUM Priority Fixes
- Split Application DTOs
- Create remaining validators
- Code cleanup

### Week 4: Final Polish
- Delete demo code
- Documentation
- Final testing and review
- Deployment preparation

---

## Risk Mitigation

### Risks:

1. **Breaking Changes**: Refactoring may break existing functionality
   - **Mitigation**: Comprehensive testing, gradual rollout

2. **Tight Timeline**: 40-60 hours is aggressive
   - **Mitigation**: Prioritize CRITICAL and HIGH items first

3. **Coordination**: Multiple files/areas affected
   - **Mitigation**: Use feature branches, regular merges

4. **Testing Gaps**: Need extensive testing
   - **Mitigation**: Write tests alongside refactoring

---

## Post-Refactoring

### Code Review Checklist

- [ ] No business logic in controllers
- [ ] All commands have validators
- [ ] One class per file throughout
- [ ] No interfaces in class files
- [ ] Proper error handling
- [ ] All tests passing
- [ ] Code coverage maintained/improved
- [ ] No console.logs or debug code
- [ ] XML documentation complete
- [ ] No hardcoded values

### Performance Testing

- [ ] API response times < 500ms
- [ ] No N+1 query issues introduced
- [ ] Database connections properly managed
- [ ] No memory leaks

### Documentation Updates

- [ ] Update API documentation
- [ ] Update architecture diagrams
- [ ] Update onboarding docs
- [ ] Update CHANGELOG.md

---

## Metrics Tracking

Track these metrics before/after refactoring:

| Metric | Before | After | Target |
|--------|--------|-------|--------|
| Controllers with business logic | 6 (35%) | 0 | 0% |
| Commands with validators | 13 (36%) | 36 | 100% |
| Files with multiple classes | 17 | 0 | 0 |
| Code coverage | ~60% | ? | >70% |
| Avg API response time | ~200ms | ? | <500ms |
| Failed builds | ? | 0 | 0 |

---

**Plan Owner**: Development Team
**Reviewers**: Tech Lead, Senior Developers
**Status**: 📋 READY FOR EXECUTION
**Next Review**: After Phase 1 Completion
