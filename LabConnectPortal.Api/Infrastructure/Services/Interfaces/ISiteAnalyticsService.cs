namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISiteAnalyticsService
{
    Task TrackVisitAsync(string? clientIp);
    Task TouchUserActivityAsync(Guid userId);
    Task<int> GetMonthlyVisitCountAsync();
    Task<int> GetOnlineUsersCountAsync();
}
