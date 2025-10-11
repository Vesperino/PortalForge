# Testing Rules - xUnit + FluentAssertions + Moq + Vitest + Playwright

## Backend Testing (xUnit + FluentAssertions + Moq)

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

### Mocking Best Practices

#### Setup Mocks

```csharp
// Setup method return value
mock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(new Employee { EmployeeId = 1 });

// Setup method with specific parameter
mock.Setup(x => x.GetByIdAsync(1))
    .ReturnsAsync(new Employee { EmployeeId = 1 });

// Setup method to throw exception
mock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
    .ThrowsAsync(new NotFoundException("Not found"));

// Setup property
mock.Setup(x => x.IsActive).Returns(true);
```

#### Verify Calls

```csharp
// Verify method was called once
mock.Verify(x => x.CreateAsync(It.IsAny<Employee>()), Times.Once);

// Verify method was never called
mock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);

// Verify method was called with specific arguments
mock.Verify(x => x.CreateAsync(It.Is<Employee>(e => e.FirstName == "John")), Times.Once);

// Verify property was accessed
mock.VerifyGet(x => x.IsActive, Times.Once);
```

### FluentAssertions Best Practices

```csharp
// Basic assertions
result.Should().NotBeNull();
result.Should().Be(expectedValue);
result.Should().BeOfType<Employee>();

// Collection assertions
collection.Should().NotBeEmpty();
collection.Should().HaveCount(5);
collection.Should().Contain(x => x.FirstName == "John");
collection.Should().BeInAscendingOrder(x => x.FirstName);

// String assertions
result.Should().StartWith("Employee");
result.Should().Contain("John");
result.Should().MatchRegex(@"^\d{3}$");

// Numeric assertions
result.Should().BeGreaterThan(0);
result.Should().BeInRange(1, 100);

// DateTime assertions
date.Should().BeAfter(DateTime.Now.AddDays(-1));
date.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));

// Exception assertions
act.Should().Throw<ArgumentNullException>()
    .WithMessage("*parameter*")
    .And.ParamName.Should().Be("request");
```

### Test Data Builders

Use builder pattern for complex test data:

```csharp
public class EmployeeBuilder
{
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string _email = "john.doe@example.com";
    private int _departmentId = 1;

    public EmployeeBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public EmployeeBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public Employee Build()
    {
        return new Employee
        {
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email,
            DepartmentId = _departmentId
        };
    }
}

// Usage in tests
var employee = new EmployeeBuilder()
    .WithFirstName("Jane")
    .WithEmail("jane@example.com")
    .Build();
```

## Frontend Testing (Vitest + Playwright)

### Unit Testing with Vitest

```typescript
// tests/composables/useEmployees.test.ts
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { useEmployees } from '~/composables/useEmployees'

describe('useEmployees', () => {
  beforeEach(() => {
    // Reset mocks before each test
    vi.clearAllMocks()
  })

  it('should fetch employees successfully', async () => {
    // Arrange
    const mockEmployees = [
      { employeeId: 1, firstName: 'John', lastName: 'Doe' },
      { employeeId: 2, firstName: 'Jane', lastName: 'Smith' }
    ]

    global.$fetch = vi.fn().mockResolvedValue({ data: mockEmployees })

    const { employees, fetchEmployees } = useEmployees()

    // Act
    await fetchEmployees()

    // Assert
    expect(employees.value).toEqual(mockEmployees)
    expect(global.$fetch).toHaveBeenCalledWith('/api/employees')
  })

  it('should handle fetch error', async () => {
    // Arrange
    global.$fetch = vi.fn().mockRejectedValue(new Error('Network error'))

    const { error, fetchEmployees } = useEmployees()

    // Act
    await fetchEmployees()

    // Assert
    expect(error.value).toBe('Failed to fetch employees')
  })
})
```

### Component Testing with Vitest

```typescript
// tests/components/EmployeeCard.test.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import EmployeeCard from '~/components/employees/EmployeeCard.vue'

describe('EmployeeCard', () => {
  it('should render employee information', () => {
    // Arrange
    const employee = {
      employeeId: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com'
    }

    // Act
    const wrapper = mount(EmployeeCard, {
      props: { employee }
    })

    // Assert
    expect(wrapper.text()).toContain('John Doe')
    expect(wrapper.text()).toContain('john.doe@example.com')
  })

  it('should emit update event when edit button is clicked', async () => {
    // Arrange
    const employee = {
      employeeId: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com'
    }

    const wrapper = mount(EmployeeCard, {
      props: { employee }
    })

    // Act
    await wrapper.find('button.edit').trigger('click')

    // Assert
    expect(wrapper.emitted('update')).toBeTruthy()
    expect(wrapper.emitted('update')?.[0]).toEqual([employee])
  })
})
```

### E2E Testing with Playwright

```typescript
// tests/e2e/employees.spec.ts
import { test, expect } from '@playwright/test'

test.describe('Employee Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/employees')
  })

  test('should display employees list', async ({ page }) => {
    // Assert
    await expect(page.getByRole('heading', { name: 'Employees' })).toBeVisible()
    await expect(page.getByRole('table')).toBeVisible()
  })

  test('should create new employee', async ({ page }) => {
    // Arrange
    await page.click('button:has-text("Add Employee")')

    // Act
    await page.fill('input[name="firstName"]', 'John')
    await page.fill('input[name="lastName"]', 'Doe')
    await page.fill('input[name="email"]', 'john.doe@example.com')
    await page.selectOption('select[name="departmentId"]', '1')
    await page.click('button:has-text("Save")')

    // Assert
    await expect(page.getByText('John Doe')).toBeVisible()
  })

  test('should validate required fields', async ({ page }) => {
    // Arrange
    await page.click('button:has-text("Add Employee")')

    // Act
    await page.click('button:has-text("Save")')

    // Assert
    await expect(page.getByText('First name is required')).toBeVisible()
    await expect(page.getByText('Email is required')).toBeVisible()
  })

  test('should delete employee with confirmation', async ({ page }) => {
    // Arrange
    const employeeRow = page.locator('tr:has-text("John Doe")')

    // Act
    await employeeRow.click('button:has-text("Delete")')
    await page.click('button:has-text("Confirm")')

    // Assert
    await expect(employeeRow).not.toBeVisible()
  })
})
```

### Test Best Practices

#### Backend Tests

1. **Use AAA pattern consistently**
2. **Mock external dependencies**
3. **Test one thing per test**
4. **Use meaningful test names**
5. **Clean up resources in Dispose/using**
6. **Test both happy path and error scenarios**
7. **Use test data builders for complex objects**
8. **Avoid testing implementation details**

#### Frontend Tests

1. **Test user behavior, not implementation**
2. **Use semantic queries** (by role, label, text)
3. **Avoid testing library code**
4. **Mock API calls**
5. **Test accessibility**
6. **Use data-testid sparingly** (prefer semantic queries)
7. **Test error states and loading states**
8. **Keep tests simple and readable**

### Code Coverage

- Aim for **>70%** code coverage for business logic
- Focus on testing critical paths
- Don't aim for 100% coverage - focus on value
- Use coverage reports to find untested code

```bash
# Backend
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura

# Frontend
npm run test:coverage
```

---

**Auto-attach**: `**/*Tests.cs`, `**/*.test.ts`, `**/*.spec.ts`
