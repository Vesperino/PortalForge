using MediatR;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Queries.GetPersonalAnalytics;

public class GetPersonalAnalyticsQuery : IRequest<PersonalAnalytics>
{
    public Guid UserId { get; set; }
    public int Year { get; set; } = DateTime.UtcNow.Year;
}