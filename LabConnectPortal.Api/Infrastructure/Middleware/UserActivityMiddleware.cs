using System.Collections.Concurrent;
using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;

namespace LabConnectPortal.Api.Infrastructure.Middleware;

public class UserActivityMiddleware(RequestDelegate next)
{
    private static readonly ConcurrentDictionary<Guid, DateTime> LastTouchCache = new();
    private static readonly TimeSpan TouchInterval = TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(HttpContext context, ISiteAnalyticsService siteAnalyticsService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdValue, out var userId))
            {
                var now = DateTime.UtcNow.ToLocalTime();
                if (!LastTouchCache.TryGetValue(userId, out var lastTouch) || now - lastTouch >= TouchInterval)
                {
                    LastTouchCache[userId] = now;
                    try
                    {
                        await siteAnalyticsService.TouchUserActivityAsync(userId);
                    }
                    catch
                    {
                        // Analytics schema may not be migrated yet; do not block requests.
                    }
                }
            }
        }

        await next(context);
    }
}
