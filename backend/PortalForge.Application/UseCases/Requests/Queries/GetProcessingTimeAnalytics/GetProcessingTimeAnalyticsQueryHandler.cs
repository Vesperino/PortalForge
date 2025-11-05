using MediatR;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Queries.GetProcessingTimeAnalytics;

public class GetProcessingTimeAnalyticsQueryHandler : IRequestHandler<GetProcessingTimeAnalyticsQuery, ProcessingTimeAnalytics>
{
    private readonly IRequestAnalyticsService _analyticsService;

    public GetProcessingTimeAnalyticsQueryHandler(IRequestAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<ProcessingTimeAnalytics> Handle(
        GetProcessingTimeAnalyticsQuery request, 
        CancellationToken cancellationToken)
    {
        return await _analyticsService.GetProcessingTimeAnalyticsAsync(
            request.UserId, 
            request.Year, 
            request.Month);
    }
}