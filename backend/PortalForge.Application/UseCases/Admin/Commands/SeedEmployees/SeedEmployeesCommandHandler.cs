using MediatR;
using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.Admin.Commands.CreateUser;

namespace PortalForge.Application.UseCases.Admin.Commands.SeedEmployees;

/// <summary>
/// Handler for seeding 40 sample employees with avatars.
/// </summary>
public class SeedEmployeesCommandHandler : IRequestHandler<SeedEmployeesCommand, SeedEmployeesResult>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SeedEmployeesCommandHandler> _logger;

    public SeedEmployeesCommandHandler(
        IMediator mediator,
        IUnitOfWork unitOfWork,
        ILogger<SeedEmployeesCommandHandler> logger)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<SeedEmployeesResult> Handle(SeedEmployeesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to seed 40 sample employees");

        var result = new SeedEmployeesResult();
        var errors = new List<string>();

        // Get first admin user as creator using GetFirstByRoleAsync
        var adminUser = await _unitOfWork.UserRepository.GetFirstByRoleAsync(
            Domain.Entities.UserRole.Admin, cancellationToken);

        if (adminUser == null)
        {
            result.Message = "No admin user found. Please create an admin first.";
            result.Errors.Add("No admin user found");
            return result;
        }

        // Get all departments for assignment
        var departments = (await _unitOfWork.DepartmentRepository.GetAllAsync()).ToList();

        // Sample employees data
        var employees = GetSampleEmployees();

        int successCount = 0;

        foreach (var emp in employees)
        {
            try
            {
                var command = new CreateUserCommand
                {
                    Email = emp.Email,
                    Password = "TempPass123!", // Default password - users should change
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    Position = emp.Position,
                    ProfilePhotoUrl = emp.AvatarUrl,
                    Department = emp.DepartmentName,
                    DepartmentId = emp.DepartmentId,
                    Role = emp.Role,
                    MustChangePassword = true,
                    CreatedBy = adminUser.Id
                };

                await _mediator.Send(command, cancellationToken);
                successCount++;
                _logger.LogInformation("Created employee: {Email}", emp.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create employee: {Email}", emp.Email);
                errors.Add($"{emp.Email}: Failed to create employee");
            }
        }

        result.EmployeesCreated = successCount;
        result.Errors = errors;
        result.Message = $"Successfully created {successCount} out of {employees.Count} employees";

        _logger.LogInformation("Seeding complete: {Count} employees created", successCount);

        return result;
    }

    private List<SampleEmployee> GetSampleEmployees()
    {
        return new List<SampleEmployee>
        {
            // IT Department - 10 employees
            new("jan.kowalski@portalforge.com", "Jan", "Kowalski", "Senior Developer", "https://randomuser.me/api/portraits/men/1.jpg", null, "IT", "Employee"),
            new("anna.nowak@portalforge.com", "Anna", "Nowak", "Frontend Developer", "https://randomuser.me/api/portraits/women/1.jpg", null, "IT", "Employee"),
            new("piotr.wisniewski@portalforge.com", "Piotr", "Wiśniewski", "Backend Developer", "https://randomuser.me/api/portraits/men/2.jpg", null, "IT", "Employee"),
            new("maria.wojcik@portalforge.com", "Maria", "Wójcik", "Full Stack Developer", "https://randomuser.me/api/portraits/women/2.jpg", null, "IT", "Employee"),
            new("tomasz.kaminski@portalforge.com", "Tomasz", "Kamiński", "DevOps Engineer", "https://randomuser.me/api/portraits/men/3.jpg", null, "IT", "Employee"),
            new("katarzyna.lewandowska@portalforge.com", "Katarzyna", "Lewandowska", "QA Engineer", "https://randomuser.me/api/portraits/women/3.jpg", null, "IT", "Employee"),
            new("michal.zielinski@portalforge.com", "Michał", "Zieliński", "Software Architect", "https://randomuser.me/api/portraits/men/4.jpg", null, "IT", "Employee"),
            new("ewa.szymanska@portalforge.com", "Ewa", "Szymańska", "UX Designer", "https://randomuser.me/api/portraits/women/4.jpg", null, "IT", "Employee"),
            new("krzysztof.duda@portalforge.com", "Krzysztof", "Duda", "Mobile Developer", "https://randomuser.me/api/portraits/men/5.jpg", null, "IT", "Employee"),
            new("magdalena.krawczyk@portalforge.com", "Magdalena", "Krawczyk", "UI Designer", "https://randomuser.me/api/portraits/women/5.jpg", null, "IT", "Employee"),

            // Marketing - 8 employees
            new("pawel.piotrowski@portalforge.com", "Paweł", "Piotrowski", "Marketing Manager", "https://randomuser.me/api/portraits/men/6.jpg", null, "Marketing", "Marketing"),
            new("agnieszka.grabowska@portalforge.com", "Agnieszka", "Grabowska", "Content Creator", "https://randomuser.me/api/portraits/women/6.jpg", null, "Marketing", "Marketing"),
            new("adam.pawlak@portalforge.com", "Adam", "Pawlak", "Social Media Specialist", "https://randomuser.me/api/portraits/men/7.jpg", null, "Marketing", "Marketing"),
            new("joanna.nowakowska@portalforge.com", "Joanna", "Nowakowska", "SEO Specialist", "https://randomuser.me/api/portraits/women/7.jpg", null, "Marketing", "Marketing"),
            new("lukasz.michalski@portalforge.com", "Łukasz", "Michalski", "Graphic Designer", "https://randomuser.me/api/portraits/men/8.jpg", null, "Marketing", "Marketing"),
            new("barbara.jankowska@portalforge.com", "Barbara", "Jankowska", "Brand Manager", "https://randomuser.me/api/portraits/women/8.jpg", null, "Marketing", "Marketing"),
            new("marcin.wojciechowski@portalforge.com", "Marcin", "Wojciechowski", "PR Specialist", "https://randomuser.me/api/portraits/men/9.jpg", null, "Marketing", "Marketing"),
            new("dorota.adamczyk@portalforge.com", "Dorota", "Adamczyk", "Marketing Analyst", "https://randomuser.me/api/portraits/women/9.jpg", null, "Marketing", "Marketing"),

            // HR - 6 employees
            new("robert.kowalczyk@portalforge.com", "Robert", "Kowalczyk", "HR Manager", "https://randomuser.me/api/portraits/men/10.jpg", null, "HR", "HR"),
            new("monika.kubiak@portalforge.com", "Monika", "Kubiak", "Recruiter", "https://randomuser.me/api/portraits/women/10.jpg", null, "HR", "HR"),
            new("grzegorz.malinowski@portalforge.com", "Grzegorz", "Malinowski", "HR Specialist", "https://randomuser.me/api/portraits/men/11.jpg", null, "HR", "HR"),
            new("alicja.kucharska@portalforge.com", "Alicja", "Kucharska", "Payroll Specialist", "https://randomuser.me/api/portraits/women/11.jpg", null, "HR", "HR"),
            new("jakub.zakrzewski@portalforge.com", "Jakub", "Zakrzewski", "Talent Acquisition", "https://randomuser.me/api/portraits/men/12.jpg", null, "HR", "HR"),
            new("izabela.mazur@portalforge.com", "Izabela", "Mazur", "Training Coordinator", "https://randomuser.me/api/portraits/women/12.jpg", null, "HR", "HR"),

            // Sales - 8 employees
            new("sebastian.olszewski@portalforge.com", "Sebastian", "Olszewski", "Sales Manager", "https://randomuser.me/api/portraits/men/13.jpg", null, "Sales", "Employee"),
            new("karolina.jasinska@portalforge.com", "Karolina", "Jasińska", "Sales Representative", "https://randomuser.me/api/portraits/women/13.jpg", null, "Sales", "Employee"),
            new("damian.walczak@portalforge.com", "Damian", "Walczak", "Account Manager", "https://randomuser.me/api/portraits/men/14.jpg", null, "Sales", "Employee"),
            new("natalia.mazurek@portalforge.com", "Natalia", "Mazurek", "Business Development", "https://randomuser.me/api/portraits/women/14.jpg", null, "Sales", "Employee"),
            new("wojciech.krol@portalforge.com", "Wojciech", "Król", "Sales Analyst", "https://randomuser.me/api/portraits/men/15.jpg", null, "Sales", "Employee"),
            new("paulina.sikora@portalforge.com", "Paulina", "Sikora", "Customer Success", "https://randomuser.me/api/portraits/women/15.jpg", null, "Sales", "Employee"),
            new("mateusz.baran@portalforge.com", "Mateusz", "Baran", "Sales Engineer", "https://randomuser.me/api/portraits/men/16.jpg", null, "Sales", "Employee"),
            new("weronika.maciejewska@portalforge.com", "Weronika", "Maciejewska", "Inside Sales", "https://randomuser.me/api/portraits/women/16.jpg", null, "Sales", "Employee"),

            // Finance - 4 employees
            new("rafal.gorski@portalforge.com", "Rafał", "Górski", "Finance Manager", "https://randomuser.me/api/portraits/men/17.jpg", null, "Finance", "Employee"),
            new("sylwia.chmielewska@portalforge.com", "Sylwia", "Chmielewska", "Accountant", "https://randomuser.me/api/portraits/women/17.jpg", null, "Finance", "Employee"),
            new("kamil.stepien@portalforge.com", "Kamil", "Stępień", "Financial Analyst", "https://randomuser.me/api/portraits/men/18.jpg", null, "Finance", "Employee"),
            new("julia.wrobel@portalforge.com", "Julia", "Wróbel", "Controller", "https://randomuser.me/api/portraits/women/18.jpg", null, "Finance", "Employee"),

            // Support - 4 employees
            new("filip.zawadzki@portalforge.com", "Filip", "Zawadzki", "Support Manager", "https://randomuser.me/api/portraits/men/19.jpg", null, "Support", "Employee"),
            new("martyna.sokolowska@portalforge.com", "Martyna", "Sokołowska", "Customer Support", "https://randomuser.me/api/portraits/women/19.jpg", null, "Support", "Employee"),
            new("bartosz.krzyzanowski@portalforge.com", "Bartosz", "Krzyżanowski", "Technical Support", "https://randomuser.me/api/portraits/men/20.jpg", null, "Support", "Employee"),
            new("oliwia.kucharska@portalforge.com", "Oliwia", "Kucharska", "Support Specialist", "https://randomuser.me/api/portraits/women/20.jpg", null, "Support", "Employee")
        };
    }

    private record SampleEmployee(
        string Email,
        string FirstName,
        string LastName,
        string Position,
        string AvatarUrl,
        Guid? DepartmentId,
        string DepartmentName,
        string Role);
}
