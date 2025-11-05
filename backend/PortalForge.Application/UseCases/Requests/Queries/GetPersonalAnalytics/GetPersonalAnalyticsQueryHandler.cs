using MediatR;
using PortalForge.Application.Interfaces;

namespace PortalForge.Application.UseCases.Requests.Queries.GetPersonalAnalytics;

public class GetPersonalAnalyticsQueryHandler : IRequestHandler<GetPersonalAnalyticsQuery, PersonalAnalytics>
{
    private readonly IRequestAnalyticsService _analyticsService;

    public GetPersonalAnalyticsQueryHandler(IRequestAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    public async Task<PersonalAnalytics> Handle(
        GetPersonalAnalyticsQuery request, 
        CancellationToken cancellationToken)
    {
        return await _analyticsService.GetPersonalAnalyticsAsync(request.UserId, request.Year);
    }
}