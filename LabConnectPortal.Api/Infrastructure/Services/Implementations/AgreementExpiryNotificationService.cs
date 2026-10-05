using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class AgreementExpiryNotificationService(
    LabConnectDbContext context,
    INotificationService notificationService) : IAgreementExpiryNotificationService
{
    private static readonly int[] WarningDays = [30, 7];

    public async Task<OperationResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Now.Date;
        var agreements = await GetExpiringAgreementsAsync(today, cancellationToken);
        var labNameCache = new Dictionary<int, string>();

        foreach (var agreement in agreements)
        {
            var daysLeft = (agreement.ExpDate.Date - today).Days;
            if (!WarningDays.Contains(daysLeft))
                continue;

            var primaryName = await ResolveLabNameAsync(agreement.PrimaryAgreementLabCodeNew, labNameCache, cancellationToken);
            var receiverName = await ResolveLabNameAsync(agreement.ReceiverAgreementLabCodeNew, labNameCache, cancellationToken);

            await NotifyLabAsync(
                agreement.PrimaryAgreementLabCodeNew,
                receiverName,
                agreement,
                daysLeft,
                cancellationToken);

            await NotifyLabAsync(
                agreement.ReceiverAgreementLabCodeNew,
                primaryName,
                agreement,
                daysLeft,
                cancellationToken);
        }

        return OperationResult.SuccessResult();
    }

    private async Task<List<LabAgreement>> GetExpiringAgreementsAsync(
        DateTime today,
        CancellationToken cancellationToken)
    {
        var warningDates = WarningDays.Select(d => today.AddDays(d)).ToList();

        return await context.LabAgreements
            .AsNoTracking()
            .Where(a =>
                !a.ParentId.HasValue
                && a.ExpDate.Date >= today
                && warningDates.Contains(a.ExpDate.Date)
                && !string.IsNullOrEmpty(a.PrimaryAgreementSign)
                && !string.IsNullOrEmpty(a.ReceiverAgreementSign))
            .ToListAsync(cancellationToken);
    }

    private async Task<string> ResolveLabNameAsync(
        int labCodeNew,
        Dictionary<int, string> cache,
        CancellationToken cancellationToken)
    {
        if (labCodeNew <= 0)
            return string.Empty;

        if (cache.TryGetValue(labCodeNew, out var cached))
            return cached;

        var result = await notificationService.ResolveCustomerLabNameAsync(labCodeNew, cancellationToken);
        var name = result.Status && !string.IsNullOrWhiteSpace(result.Data)
            ? result.Data!
            : labCodeNew.ToString();

        cache[labCodeNew] = name;
        return name;
    }

    private async Task NotifyLabAsync(
        int labCodeNew,
        string counterpartyName,
        LabAgreement agreement,
        int daysLeft,
        CancellationToken cancellationToken)
    {
        if (labCodeNew <= 0)
            return;

        try
        {
            var (title, message) = BuildMessages(agreement, counterpartyName, daysLeft);
            var detailUrl = $"/profile/agreements?agreementId={agreement.Id}";

            var adminUserId = await context.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.AdminLab
                    && u.CenterProfile != null
                    && u.CenterProfile.LabCodeNew == labCodeNew)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (adminUserId != Guid.Empty)
            {
                await notificationService.CreateAsync(
                    adminUserId,
                    title,
                    message,
                    NotificationType.General);
            }

            await notificationService.SendExternalNotificationByLabCodesAsync(
                new NotificationCommandBase
                {
                    Title = title,
                    Message = message,
                    IsActive = true,
                    Priority = daysLeft == 7 ? 2 : 1,
                    NotificationActions =
                    [
                        new NotificationAction
                        {
                            NotificationActionType = NotificationActionType.OpenUrl,
                            Label = "مشاهده قرارداد",
                            Url = detailUrl,
                        },
                    ],
                },
                [labCodeNew],
                cancellationToken);
        }
        catch
        {
            // ادامه اعلان برای سایر آزمایشگاه‌ها
        }
    }

    private static (string Title, string Message) BuildMessages(
        LabAgreement agreement,
        string counterpartyName,
        int daysLeft)
    {
        var expDate = agreement.ExpDate.ToString("yyyy/MM/dd");
        var counterparty = string.IsNullOrWhiteSpace(counterpartyName)
            ? "طرف مقابل"
            : counterpartyName;

        if (daysLeft == 30)
        {
            return (
                $"نزدیک شدن به پایان قرارداد شماره {agreement.ContractNumber}",
                $"قرارداد شما با {counterparty} در تاریخ {expDate} به پایان می‌رسد. برای درخواست تمدید، روی لینک زیر کلیک کنید.");
        }

        return (
            $"هشدار: قرارداد شماره {agreement.ContractNumber} در ۷ روز آینده منقضی می‌شود",
            $"قرارداد شما با {counterparty} در تاریخ {expDate} (۷ روز دیگر) منقضی می‌شود. برای درخواست تمدید، روی لینک زیر کلیک کنید.");
    }
}
