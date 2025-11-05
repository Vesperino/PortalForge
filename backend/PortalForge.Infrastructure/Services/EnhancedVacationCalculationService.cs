using Microsoft.Extensions.Logging;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.DTOs;
using PortalForge.Application.Interfaces;
using PortalForge.Domain.Enums;

namespace PortalForge.Infrastructure.Services;

/// <summary>
/// Enhanced vacation calculation service that extends the base VacationCalculationService
/// with Polish labor law compliance features, conflict detection, and advanced validation.
/// </summary>
public class EnhancedVacationCalculationService : VacationCalculationService, IEnhancedVacationCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EnhancedVacationCalculationService> _logger;

    // Polish labor law constants
    private const int MAX_ON_DEMAND_DAYS_PER_YEAR = 4;
    private const double TEAM_COVERAGE_WARNING_THRESHOLD = 0.7; // 70% coverage minimum
    private const double TEAM_COVERAGE_CRITICAL_THRESHOLD = 0.5; // 50% coverage critical

    // Circumstantial leave reasons and their allowed days (Polish labor law)
    private static readonly Dictionary<string, (int MaxDays, bool RequiresDocumentation)> CircumstantialLeaveReasons = new()
    {
        { "wedding", (2, true) }, // Own wedding
        { "marriage", (2, true) }, // Own marriage
        { "birth", (2, true) }, // Birth of child
        { "child_birth", (2, true) }, // Birth of child
        { "funeral", (2, true) }, // Death of close family member
        { "death", (2, true) }, // Death of close family member
        { "family_funeral", (2, true) }, // Family funeral
        { "medical", (1, true) }, // Medical procedures for family
        { "moving", (1, false) }, // Moving residence
        { "other", (2, true) } // Other circumstances requiring documentation
    };

    public EnhancedVacationCalculationService(
        IUnitOfWork unitOfWork,
        ILogger<EnhancedVacationCalculationService> logger)
        : base(unitOfWork, logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CircumstantialLeaveResult> ValidateCircumstantialLeaveAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        string reason,
        bool hasDocumentation)
    {
        _logger.LogInformation(
            "Validating circumstantial leave for user {UserId}: {Reason}, {StartDate} - {EndDate}, HasDoc: {HasDoc}",
            userId, reason, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), hasDocumentation);

        try
        {
            // Calculate requested days
            var requestedDays = CalculateBusinessDays(startDate, endDate);
            
            if (requestedDays <= 0)
            {
                return CircumstantialLeaveResult.Failure("Nieprawidłowy zakres dat urlopu", requestedDays);
            }

            // Normalize reason to match our dictionary
            var normalizedReason = NormalizeCircumstantialReason(reason);
            
            if (!CircumstantialLeaveReasons.TryGetValue(normalizedReason, out var leaveInfo))
            {
                _logger.LogWarning("Unknown circumstantial leave reason: {Reason}", reason);
                normalizedReason = "other";
                leaveInfo = CircumstantialLeaveReasons["other"];
            }

            // Check if requested days exceed maximum allowed
            if (requestedDays > leaveInfo.MaxDays)
            {
                return CircumstantialLeaveResult.Failure(
                    $"Urlop okolicznościowy typu '{normalizedReason}' może trwać maksymalnie {leaveInfo.MaxDays} dni. Żądano: {requestedDays} dni.",
                    requestedDays);
            }

            // Check documentation requirements
            if (leaveInfo.RequiresDocumentation && !hasDocumentation)
            {
                return CircumstantialLeaveResult.Failure(
                    $"Urlop okolicznościowy typu '{normalizedReason}' wymaga załączenia dokumentacji potwierdzającej.",
                    requestedDays);
            }

            // Check user's circumstantial leave usage this year
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return CircumstantialLeaveResult.Failure("Użytkownik nie istnieje", requestedDays);
            }

            var usedThisYear = user.CircumstantialLeaveDaysUsed ?? 0;
            var additionalInfo = usedThisYear > 0 
                ? $"Wykorzystano już {usedThisYear} dni urlopu okolicznościowego w tym roku."
                : null;

            return CircumstantialLeaveResult.Success(
                requestedDays,
                normalizedReason,
                leaveInfo.MaxDays,
                leaveInfo.RequiresDocumentation,
                additionalInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating circumstantial leave for user {UserId}", userId);
            return CircumstantialLeaveResult.Failure("Błąd podczas walidacji urlopu okolicznościowego");
        }
    }

    public async Task<OnDemandVacationResult> ValidateOnDemandVacationAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInformation(
            "Validating on-demand vacation for user {UserId}: {StartDate} - {EndDate}",
            userId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        try
        {
            var requestedDays = CalculateBusinessDays(startDate, endDate);
            var currentYear = DateTime.UtcNow.Year;

            if (requestedDays <= 0)
            {
                return OnDemandVacationResult.Failure(
                    "Nieprawidłowy zakres dat urlopu", requestedDays, 0, currentYear);
            }

            // Get user's current on-demand vacation usage
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return OnDemandVacationResult.Failure(
                    "Użytkownik nie istnieje", requestedDays, 0, currentYear);
            }

            var usedThisYear = user.OnDemandVacationDaysUsed ?? 0;
            var remainingDays = MAX_ON_DEMAND_DAYS_PER_YEAR - usedThisYear;

            // Check if user has enough on-demand days remaining
            if (requestedDays > remainingDays)
            {
                return OnDemandVacationResult.Failure(
                    $"Brak wystarczającej liczby dni urlopu na żądanie. Dostępne: {remainingDays} dni, żądano: {requestedDays} dni.",
                    requestedDays, usedThisYear, currentYear);
            }

            // Check if user has exhausted their annual limit
            if (usedThisYear >= MAX_ON_DEMAND_DAYS_PER_YEAR)
            {
                return OnDemandVacationResult.Failure(
                    "Wykorzystano już wszystkie 4 dni urlopu na żądanie w tym roku.",
                    requestedDays, usedThisYear, currentYear);
            }

            var additionalInfo = remainingDays - requestedDays > 0
                ? $"Po tym urlopie pozostanie {remainingDays - requestedDays} dni urlopu na żądanie."
                : "To będą ostatnie dni urlopu na żądanie w tym roku.";

            return OnDemandVacationResult.Success(
                requestedDays, usedThisYear, currentYear, additionalInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating on-demand vacation for user {UserId}", userId);
            return OnDemandVacationResult.Failure(
                "Błąd podczas walidacji urlopu na żądanie", 0, 0, DateTime.UtcNow.Year);
        }
    }

    public async Task<VacationConflictResult> CheckVacationConflictsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInformation(
            "Checking vacation conflicts for user {UserId}: {StartDate} - {EndDate}",
            userId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        try
        {
            var conflicts = new List<VacationConflict>();
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return VacationConflictResult.WithConflicts(
                    new List<VacationConflict>
                    {
                        new VacationConflict
                        {
                            Type = VacationConflictType.OverlappingVacation,
                            Description = "Użytkownik nie istnieje",
                            Severity = ConflictSeverity.Critical
                        }
                    },
                    new TeamCoverageAnalysis());
            }

            // 1. Check for overlapping vacations for the same user
            await CheckOverlappingVacations(userId, startDate, endDate, conflicts);

            // 2. Check team coverage if user has a department
            TeamCoverageAnalysis coverageAnalysis = new();
            if (user.DepartmentId.HasValue)
            {
                coverageAnalysis = await AnalyzeTeamCoverage(user.DepartmentId.Value, startDate, endDate, userId);
                
                // Add coverage conflicts if any
                if (!coverageAnalysis.IsAdequateCoverage)
                {
                    conflicts.Add(new VacationConflict
                    {
                        Type = VacationConflictType.InsufficientCoverage,
                        Description = $"Niewystarczające pokrycie zespołu: {coverageAnalysis.CoveragePercentage:F1}% dostępności",
                        ConflictStartDate = startDate,
                        ConflictEndDate = endDate,
                        Severity = coverageAnalysis.CoveragePercentage < 50 ? ConflictSeverity.Critical : ConflictSeverity.Medium
                    });
                }
            }

            // 3. Check if supervisor is on vacation (if user has supervisor)
            if (user.SupervisorId.HasValue)
            {
                await CheckSupervisorAvailability(user.SupervisorId.Value, startDate, endDate, conflicts);
            }

            // Determine if vacation can be approved despite conflicts
            var canBeApproved = DetermineApprovalStatus(conflicts);

            if (conflicts.Any())
            {
                return VacationConflictResult.WithConflicts(conflicts, coverageAnalysis, canBeApproved);
            }

            return VacationConflictResult.NoConflicts(coverageAnalysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking vacation conflicts for user {UserId}", userId);
            return VacationConflictResult.WithConflicts(
                new List<VacationConflict>
                {
                    new VacationConflict
                    {
                        Type = VacationConflictType.OverlappingVacation,
                        Description = "Błąd podczas sprawdzania konfliktów urlopowych",
                        Severity = ConflictSeverity.High
                    }
                },
                new TeamCoverageAnalysis());
        }
    }

    public async Task<int> GetRemainingOnDemandDaysAsync(Guid userId, int year = 0)
    {
        if (year == 0)
        {
            year = DateTime.UtcNow.Year;
        }

        _logger.LogDebug("Getting remaining on-demand days for user {UserId} in year {Year}", userId, year);

        try
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when getting remaining on-demand days", userId);
                return 0;
            }

            var usedThisYear = user.OnDemandVacationDaysUsed ?? 0;
            var remaining = Math.Max(0, MAX_ON_DEMAND_DAYS_PER_YEAR - usedThisYear);

            _logger.LogDebug(
                "User {UserId} has {Remaining} on-demand vacation days remaining in {Year} (used: {Used})",
                userId, remaining, year, usedThisYear);

            return remaining;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining on-demand days for user {UserId}", userId);
            return 0;
        }
    }

    #region Private Helper Methods

    private string NormalizeCircumstantialReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return "other";

        var normalized = reason.ToLowerInvariant().Trim();
        
        // Map common variations to standard reasons
        var mappings = new Dictionary<string, string>
        {
            { "ślub", "wedding" },
            { "wesele", "wedding" },
            { "małżeństwo", "marriage" },
            { "narodziny", "birth" },
            { "poród", "birth" },
            { "dziecko", "birth" },
            { "pogrzeb", "funeral" },
            { "śmierć", "death" },
            { "zgon", "death" },
            { "przeprowadzka", "moving" },
            { "lekarz", "medical" },
            { "szpital", "medical" }
        };

        return mappings.TryGetValue(normalized, out var mapped) ? mapped : normalized;
    }

    private async Task CheckOverlappingVacations(Guid userId, DateTime startDate, DateTime endDate, List<VacationConflict> conflicts)
    {
        var userVacations = await _unitOfWork.VacationScheduleRepository.GetByUserAsync(userId);
        
        var overlapping = userVacations.Where(v => 
            (v.Status == VacationStatus.Scheduled || v.Status == VacationStatus.Active) &&
            v.StartDate <= endDate && v.EndDate >= startDate).ToList();

        foreach (var vacation in overlapping)
        {
            conflicts.Add(new VacationConflict
            {
                Type = VacationConflictType.OverlappingVacation,
                Description = $"Nakładający się urlop: {vacation.StartDate:yyyy-MM-dd} - {vacation.EndDate:yyyy-MM-dd}",
                ConflictStartDate = vacation.StartDate,
                ConflictEndDate = vacation.EndDate,
                Severity = ConflictSeverity.Critical
            });
        }
    }

    private async Task<TeamCoverageAnalysis> AnalyzeTeamCoverage(Guid departmentId, DateTime startDate, DateTime endDate, Guid requestingUserId)
    {
        // Get team vacations in the date range
        var teamVacations = await _unitOfWork.VacationScheduleRepository.GetTeamVacationsAsync(
            departmentId, startDate, endDate);

        // Get all active users in the department
        var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
        var teamMembers = allUsers.Where(u => u.DepartmentId == departmentId && u.IsActive).ToList();
        var teamSize = teamMembers.Count;

        if (teamSize == 0)
        {
            return new TeamCoverageAnalysis
            {
                TeamSize = 0,
                IsAdequateCoverage = true
            };
        }

        // Count unique users on vacation (excluding the requesting user)
        var usersOnVacation = teamVacations
            .Where(v => v.UserId != requestingUserId && 
                       (v.Status == VacationStatus.Scheduled || v.Status == VacationStatus.Active))
            .Select(v => v.UserId)
            .Distinct()
            .Count();

        // Add 1 for the requesting user
        var totalOnVacation = usersOnVacation + 1;
        var membersAvailable = Math.Max(0, teamSize - totalOnVacation);
        var coveragePercentage = teamSize > 0 ? (double)membersAvailable / teamSize * 100 : 100;

        // Find critical coverage dates
        var criticalDates = new List<DateTime>();
        var currentDate = startDate.Date;
        while (currentDate <= endDate.Date)
        {
            var onVacationThisDate = teamVacations.Count(v => 
                v.StartDate <= currentDate && v.EndDate >= currentDate) + 1; // +1 for requesting user
            
            var coverageThisDate = (double)(teamSize - onVacationThisDate) / teamSize;
            if (coverageThisDate < TEAM_COVERAGE_CRITICAL_THRESHOLD)
            {
                criticalDates.Add(currentDate);
            }
            
            currentDate = currentDate.AddDays(1);
        }

        return new TeamCoverageAnalysis
        {
            TeamSize = teamSize,
            MembersOnVacation = totalOnVacation,
            MembersAvailable = membersAvailable,
            CoveragePercentage = coveragePercentage,
            IsAdequateCoverage = coveragePercentage >= (TEAM_COVERAGE_WARNING_THRESHOLD * 100),
            CriticalCoverageDates = criticalDates
        };
    }

    private async Task CheckSupervisorAvailability(Guid supervisorId, DateTime startDate, DateTime endDate, List<VacationConflict> conflicts)
    {
        var supervisorVacations = await _unitOfWork.VacationScheduleRepository.GetByUserAsync(supervisorId);
        
        var overlapping = supervisorVacations.Where(v => 
            (v.Status == VacationStatus.Scheduled || v.Status == VacationStatus.Active) &&
            v.StartDate <= endDate && v.EndDate >= startDate).ToList();

        foreach (var vacation in overlapping)
        {
            conflicts.Add(new VacationConflict
            {
                Type = VacationConflictType.KeyPersonnelUnavailable,
                Description = $"Przełożony niedostępny: {vacation.StartDate:yyyy-MM-dd} - {vacation.EndDate:yyyy-MM-dd}",
                ConflictStartDate = vacation.StartDate,
                ConflictEndDate = vacation.EndDate,
                Severity = ConflictSeverity.Medium
            });
        }
    }

    private bool DetermineApprovalStatus(List<VacationConflict> conflicts)
    {
        // Cannot approve if there are critical conflicts
        if (conflicts.Any(c => c.Severity == ConflictSeverity.Critical))
        {
            return false;
        }

        // Can approve if only low or medium severity conflicts
        return !conflicts.Any(c => c.Severity == ConflictSeverity.High);
    }

    #endregion
}