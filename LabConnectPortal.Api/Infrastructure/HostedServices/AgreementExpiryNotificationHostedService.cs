using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using Microsoft.Extensions.Options;

namespace LabConnectPortal.Api.Infrastructure.HostedServices;

public class AgreementExpiryNotificationHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<AgreementExpiryNotificationSettings> settings,
    ILogger<AgreementExpiryNotificationHostedService> logger) : BackgroundService
{
    private readonly AgreementExpiryNotificationSettings _settings = settings.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            logger.LogInformation("Agreement expiry notifications are disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            logger.LogInformation(
                "Agreement expiry notification job scheduled in {Delay} (next run at {RunAtHour:D2}:{RunAtMinute:D2}).",
                delay,
                _settings.RunAtHour,
                _settings.RunAtMinute);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            await RunJobAsync(stoppingToken);
        }
    }

    private async Task RunJobAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAgreementExpiryNotificationService>();
            var result = await service.RunAsync(stoppingToken);

            if (!result.Status)
                logger.LogWarning("Agreement expiry notification job failed: {Message}", result.Message);
            else
                logger.LogInformation("Agreement expiry notification job completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Agreement expiry notification job threw an exception.");
        }
    }

    private TimeSpan GetDelayUntilNextRun()
    {
        var timeZone = ResolveTimeZone();
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        var nextRun = new DateTimeOffset(
            now.Year,
            now.Month,
            now.Day,
            _settings.RunAtHour,
            _settings.RunAtMinute,
            0,
            now.Offset);

        if (nextRun <= now)
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }

    private TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(_settings.TimeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Local;
        }
    }
}
