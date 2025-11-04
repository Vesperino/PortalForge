using Hangfire.Dashboard;

namespace PortalForge.Api.Middleware;

/// <summary>
/// Authorization filter for Hangfire Dashboard.
/// Only authenticated users can access the dashboard.
/// In production, you may want to restrict this to Admin role only.
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow authenticated users to access dashboard
        // TODO: In production, restrict to Admin role only:
        // return httpContext.User.Identity?.IsAuthenticated == true &&
        //        httpContext.User.IsInRole("Admin");

        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}
