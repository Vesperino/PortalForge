using MediatR;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Queries.GetProcessingTimeAnalytics;

public class GetProcessingTimeAnalyticsQuery : IRequest<ProcessingTimeAnalytics>
{
    public Guid UserId { get; set; }
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public int? Month { get; set; }
}