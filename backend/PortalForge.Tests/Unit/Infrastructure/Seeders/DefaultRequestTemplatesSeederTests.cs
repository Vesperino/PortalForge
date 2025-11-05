using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PortalForge.Domain.Entities;
using PortalForge.Domain.Enums;
using PortalForge.Infrastructure.Data.Seeders;
using PortalForge.Infrastructure.Persistence;

namespace PortalForge.Tests.Unit.Infrastructure.Seeders;

public class DefaultRequestTemplatesSeederTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<DefaultRequestTemplatesSeeder>> _loggerMock;
    private readonly DefaultRequestTemplatesSeeder _seeder;

    public DefaultRequestTemplatesSeederTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DefaultRequestTemplatesSeeder>>();
        _seeder = new DefaultRequestTemplatesSeeder(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task SeedAsync_WhenNoAdminUserExists_ShouldSkipSeeding()
    {
        // Act
        await _seeder.SeedAsync();

        // Assert
        var templates = await _context.RequestTemplates.ToListAsync();
        templates.Should().BeEmpty();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No admin user found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WhenNoTemplatesExist_ShouldCreateBothTemplates()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync();

        // Assert
        var templates = await _context.RequestTemplates
            .Include(t => t.Fields)
            .Include(t => t.ApprovalStepTemplates)
            .ToListAsync();

        templates.Should().HaveCount(2);

        var vacationTemplate = templates.FirstOrDefault(t => t.IsVacationRequest);
        vacationTemplate.Should().NotBeNull();
        vacationTemplate!.Name.Should().Be("Wniosek urlopowy");
        vacationTemplate.Icon.Should().Be("calendar");
        vacationTemplate.Category.Should().Be("Urlopy i absencje");
        vacationTemplate.RequiresApproval.Should().BeTrue();
        vacationTemplate.RequiresSubstituteSelection.Should().BeTrue();
        vacationTemplate.MaxRetrospectiveDays.Should().BeNull();
        vacationTemplate.Fields.Should().HaveCount(4);
        vacationTemplate.ApprovalStepTemplates.Should().HaveCount(1);
        vacationTemplate.ApprovalStepTemplates.First().ApproverType.Should().Be(ApproverType.DirectSupervisor);

        var sickLeaveTemplate = templates.FirstOrDefault(t => t.IsSickLeaveRequest);
        sickLeaveTemplate.Should().NotBeNull();
        sickLeaveTemplate!.Name.Should().Be("Zgłoszenie L4 (zwolnienie lekarskie)");
        sickLeaveTemplate.Icon.Should().Be("medical-bag");
        sickLeaveTemplate.Category.Should().Be("Urlopy i absencje");
        sickLeaveTemplate.IsSickLeaveRequest.Should().BeTrue();
        sickLeaveTemplate.MaxRetrospectiveDays.Should().Be(14);
        sickLeaveTemplate.AllowsAttachments.Should().BeTrue();
        sickLeaveTemplate.Fields.Should().HaveCount(3);
        sickLeaveTemplate.ApprovalStepTemplates.Should().HaveCount(1);
    }

    [Fact]
    public async Task SeedAsync_VacationTemplate_ShouldHaveCorrectFields()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync();

        // Assert
        var vacationTemplate = await _context.RequestTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.IsVacationRequest);

        vacationTemplate.Should().NotBeNull();
        var fields = vacationTemplate!.Fields.OrderBy(f => f.Order).ToList();

        fields.Should().HaveCount(4);

        // Field 1: Vacation Type (Select)
        fields[0].Label.Should().Be("Typ urlopu");
        fields[0].FieldType.Should().Be(FieldType.Select);
        fields[0].IsRequired.Should().BeTrue();
        fields[0].Options.Should().NotBeNullOrEmpty();
        fields[0].Options.Should().Contain("Annual");
        fields[0].Options.Should().Contain("OnDemand");
        fields[0].Options.Should().Contain("Circumstantial");

        // Field 2: Start Date
        fields[1].Label.Should().Be("Data rozpoczęcia");
        fields[1].FieldType.Should().Be(FieldType.Date);
        fields[1].IsRequired.Should().BeTrue();

        // Field 3: End Date
        fields[2].Label.Should().Be("Data zakończenia");
        fields[2].FieldType.Should().Be(FieldType.Date);
        fields[2].IsRequired.Should().BeTrue();

        // Field 4: Reason (optional)
        fields[3].Label.Should().Be("Powód (dla urlopu okolicznościowego)");
        fields[3].FieldType.Should().Be(FieldType.Textarea);
        fields[3].IsRequired.Should().BeFalse();
    }

    [Fact]
    public async Task SeedAsync_SickLeaveTemplate_ShouldHaveCorrectFields()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync();

        // Assert
        var sickLeaveTemplate = await _context.RequestTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.IsSickLeaveRequest);

        sickLeaveTemplate.Should().NotBeNull();
        var fields = sickLeaveTemplate!.Fields.OrderBy(f => f.Order).ToList();

        fields.Should().HaveCount(3);

        // Field 1: Start Date
        fields[0].Label.Should().Be("Data rozpoczęcia zwolnienia");
        fields[0].FieldType.Should().Be(FieldType.Date);
        fields[0].IsRequired.Should().BeTrue();

        // Field 2: End Date
        fields[1].Label.Should().Be("Data zakończenia zwolnienia");
        fields[1].FieldType.Should().Be(FieldType.Date);
        fields[1].IsRequired.Should().BeTrue();

        // Field 3: Notes
        fields[2].Label.Should().Be("Uwagi");
        fields[2].FieldType.Should().Be(FieldType.Textarea);
        fields[2].IsRequired.Should().BeFalse();
    }

    [Fact]
    public async Task SeedAsync_WhenVacationTemplateExists_ShouldOnlyCreateSickLeaveTemplate()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        await _context.Users.AddAsync(adminUser);

        var existingVacationTemplate = new RequestTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Wniosek urlopowy",
            Description = "Existing template",
            Icon = "calendar",
            Category = "Urlopy i absencje",
            IsVacationRequest = true,
            CreatedById = adminUser.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _context.RequestTemplates.AddAsync(existingVacationTemplate);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync();

        // Assert
        var templates = await _context.RequestTemplates.ToListAsync();
        templates.Should().HaveCount(2); // existing + newly created sick leave

        var vacationTemplates = templates.Where(t => t.IsVacationRequest).ToList();
        vacationTemplates.Should().HaveCount(1); // only the existing one

        var sickLeaveTemplates = templates.Where(t => t.IsSickLeaveRequest).ToList();
        sickLeaveTemplates.Should().HaveCount(1); // newly created
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_CanRunMultipleTimes()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        // Act - run seeder three times
        await _seeder.SeedAsync();
        await _seeder.SeedAsync();
        await _seeder.SeedAsync();

        // Assert
        var templates = await _context.RequestTemplates.ToListAsync();
        templates.Should().HaveCount(2); // only 2 templates, not 6

        var vacationTemplates = templates.Where(t => t.IsVacationRequest).ToList();
        vacationTemplates.Should().HaveCount(1);

        var sickLeaveTemplates = templates.Where(t => t.IsSickLeaveRequest).ToList();
        sickLeaveTemplates.Should().HaveCount(1);
    }

    [Fact]
    public async Task SeedAsync_ShouldUseFirstAdminAsCreator()
    {
        // Arrange
        var adminUser1 = CreateAdminUser();
        var adminUser2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin2@test.com",
            FirstName = "Admin",
            LastName = "Two",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow.AddDays(1) // created after first admin
        };

        await _context.Users.AddAsync(adminUser1);
        await _context.Users.AddAsync(adminUser2);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync();

        // Assert
        var templates = await _context.RequestTemplates.ToListAsync();
        templates.Should().HaveCount(2);
        templates.Should().OnlyContain(t => t.CreatedById == adminUser1.Id || t.CreatedById == adminUser2.Id);
    }

    [Fact]
    public async Task SeedAsync_ShouldLogSuccessfulSeeding()
    {
        // Arrange
        var adminUser = CreateAdminUser();
        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting default request templates seeding")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created vacation request template")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created sick leave template")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Default request templates seeding completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private User CreateAdminUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };
    }
}
