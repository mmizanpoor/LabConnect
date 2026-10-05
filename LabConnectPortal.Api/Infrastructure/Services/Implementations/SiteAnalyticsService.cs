using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SiteAnalyticsService(LabConnectDbContext context, IConfiguration configuration) : ISiteAnalyticsService
{
    private const int OnlineWindowMinutes = 5;

    private string IpHashPepper =>
        configuration["ViewTracking:IpHashPepper"]
        ?? throw new InvalidOperationException("ViewTracking:IpHashPepper is not configured.");

    public async Task TrackVisitAsync(string? clientIp)
    {
        if (string.IsNullOrWhiteSpace(clientIp))
            return;

        try
        {
            var ipHash = ClientIpHelper.HashIp(clientIp, IpHashPepper);
            var dayStart = DateTime.UtcNow.Date;

            var alreadyVisitedToday = await context.SiteVisits.AsNoTracking()
                .AnyAsync(v => v.IpHash == ipHash && v.ViewedAt >= dayStart);

            if (alreadyVisitedToday)
                return;

            context.SiteVisits.Add(new SiteVisit
            {
                Id = Guid.NewGuid(),
                IpHash = ipHash,
                ViewedAt = DateTime.UtcNow,
            });

            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Ignore duplicate visit races for the same IP/day.
        }
        catch
        {
            // Analytics schema may not be migrated yet.
        }
    }

    public async Task TouchUserActivityAsync(Guid userId)
    {
        var now = DateTime.UtcNow.ToLocalTime();
        await context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.LastSeenAt, now));
    }

    public async Task<int> GetMonthlyVisitCountAsync()
    {
        try
        {
            var (fromShamsi, toShamsi) = CustomConverter.GetShamsiMonthRange(DateTime.Now);
            var from = fromShamsi.GetShamsiDatePart().ShamsiStrToMiladiDate();
            var to = toShamsi.GetShamsiDatePart().ShamsiStrToMiladiDate()
                .Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            return await context.SiteVisits.AsNoTracking()
                .CountAsync(v => v.ViewedAt >= from && v.ViewedAt <= to);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetOnlineUsersCountAsync()
    {
        try
        {
            var threshold = DateTime.UtcNow.AddMinutes(-OnlineWindowMinutes);
            return await context.Users.AsNoTracking()
                .CountAsync(u => u.IsActive && u.LastSeenAt != null && u.LastSeenAt >= threshold);
        }
        catch
        {
            return 0;
        }
    }
}
