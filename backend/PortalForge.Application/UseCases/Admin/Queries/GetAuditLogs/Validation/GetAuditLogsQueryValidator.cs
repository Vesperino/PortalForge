using FluentValidation;

namespace PortalForge.Application.UseCases.Admin.Queries.GetAuditLogs.Validation;

/// <summary>
/// Validator for GetAuditLogsQuery.
/// Ensures pagination parameters and date ranges are valid.
/// </summary>
public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Numer strony musi być większy niż 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Rozmiar strony musi być większy niż 0")
            .LessThanOrEqualTo(100).WithMessage("Rozmiar strony nie może przekraczać 100 elementów");

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.ToDate >= x.FromDate)
            .WithMessage("Data końcowa musi być późniejsza lub równa dacie początkowej");

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || (x.ToDate.Value - x.FromDate.Value).TotalDays <= 365)
            .WithMessage("Zakres dat nie może przekraczać 365 dni");

        RuleFor(x => x.EntityType)
            .MaximumLength(100).WithMessage("Typ encji nie może przekraczać 100 znaków")
            .When(x => !string.IsNullOrEmpty(x.EntityType));

        RuleFor(x => x.Action)
            .MaximumLength(100).WithMessage("Akcja nie może przekraczać 100 znaków")
            .When(x => !string.IsNullOrEmpty(x.Action));
    }
}
