using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class PublicSpecialOfferService(
    LabConnectDbContext context,
    INotificationService notificationService,
    IUserRepository userRepository) : IPublicSpecialOfferService
{
    public async Task<OperationResult<List<PublicSpecialOfferCardDto>>> GetActiveForHomeAsync()
    {
        var today = DateTime.UtcNow.Date;
        var offers = await context.SpecialOffers
            .AsNoTracking()
            .Where(o => o.IsActive && o.EndDate.Date >= today && o.StartDate.Date <= today)
            .OrderByDescending(o => o.EndDate)
            .Take(12)
            .Select(o => new
            {
                o.Id,
                o.Title,
                o.Summary,
                o.EndDate,
                o.LabCodeNew,
            })
            .ToListAsync();

        var labSummaries = await ResolveLabSummariesAsync(offers.Select(o => o.LabCodeNew).Distinct().ToList());

        var items = offers.Select(o =>
        {
            var lab = labSummaries.GetValueOrDefault(o.LabCodeNew);
            return new PublicSpecialOfferCardDto
            {
                Id = o.Id,
                Title = o.Title,
                Summary = o.Summary,
                EndDate = o.EndDate,
                LabName = lab?.Name ?? string.Empty,
                LabProfileId = lab?.ProfileId,
                HasLabLogo = lab?.HasLogo ?? false,
            };
        }).ToList();

        return OperationResult<List<PublicSpecialOfferCardDto>>.Success(items);
    }

    public async Task<OperationResult<PublicSpecialOfferDetailDto>> GetDetailAsync(long id, Guid? userId)
    {
        var today = DateTime.UtcNow.Date;
        var offer = await context.SpecialOffers
            .AsNoTracking()
            .Include(o => o.Tests)
            .ThenInclude(t => t.TestInfo)
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive && o.EndDate.Date >= today);

        if (offer == null)
            return OperationResult<PublicSpecialOfferDetailDto>.Failure("پیشنهاد یافت نشد");

        var labSummaries = await ResolveLabSummariesAsync([offer.LabCodeNew]);
        var user = userId.HasValue
            ? await userRepository.GetWithRolesAsync(userId.Value)
            : null;

        var hasSubmitted = userId.HasValue && await context.SpecialOfferRequests
            .AsNoTracking()
            .AnyAsync(r => r.SpecialOfferId == id && r.UserId == userId.Value);

        var isOwnLabOffer = IsOwnLabOffer(user, offer.LabCodeNew);
        var canSubmit = CanUserSubmitRequest(user, offer.LabCodeNew, hasSubmitted);
        var labSummary = labSummaries.GetValueOrDefault(offer.LabCodeNew);

        return OperationResult<PublicSpecialOfferDetailDto>.Success(new PublicSpecialOfferDetailDto
        {
            Id = offer.Id,
            Title = offer.Title,
            Summary = offer.Summary,
            FullBody = offer.FullBody,
            StartDate = offer.StartDate,
            EndDate = offer.EndDate,
            LabName = labSummary?.Name ?? string.Empty,
            ProposerFirstName = labSummary?.FirstName ?? string.Empty,
            LabCodeNew = offer.LabCodeNew,
            HasSubmittedRequest = hasSubmitted,
            CanSubmitRequest = canSubmit,
            IsOwnLabOffer = isOwnLabOffer,
            Tests = offer.Tests
                .OrderBy(t => t.TestInfo.FullName)
                .Select(t => new PublicSpecialOfferTestDto
                {
                    CpnCode = t.TestInfo.CPNCode,
                    NationalCode = t.TestInfo.NationalCode,
                    FullName = t.TestInfo.FullName,
                    ShortName = t.TestInfo.ShortName,
                    SectionName = t.TestInfo.SectionName,
                    ApprovePrice = t.TestInfo.ApprovePrice,
                    Discount = t.Discount,
                    MaxSamples = t.MaxSamples,
                })
                .ToList(),
        });
    }

    public async Task<OperationResult> SubmitRequestAsync(long id, Guid userId, SubmitSpecialOfferRequestCommand command)
    {
        var today = DateTime.UtcNow.Date;
        var offer = await context.SpecialOffers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.IsActive && o.EndDate.Date >= today);

        if (offer == null)
            return OperationResult.Failure("پیشنهاد یافت نشد یا منقضی شده است");

        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return OperationResult.Failure("کاربر یافت نشد");

        if (user.UserType != UserType.UserLab && user.UserType != UserType.AdminLab)
            return OperationResult.Failure("فقط کاربران آزمایشگاه می‌توانند درخواست ثبت کنند");

        if (!user.GetLabCodeNew().HasValue)
            return OperationResult.Failure("کد آزمایشگاه کاربر تعریف نشده است");

        if (IsOwnLabOffer(user, offer.LabCodeNew))
            return OperationResult.Failure("امکان ثبت درخواست برای پیشنهاد آزمایشگاه خود وجود ندارد");

        var alreadySubmitted = await context.SpecialOfferRequests
            .AnyAsync(r => r.SpecialOfferId == id && r.UserId == userId);

        if (alreadySubmitted)
            return OperationResult.Failure("شما قبلاً برای این پیشنهاد درخواست ثبت کرده‌اید");

        var description = command.Description?.Trim() ?? string.Empty;
        if (description.Length > 2000)
            return OperationResult.Failure("توضیحات حداکثر ۲۰۰۰ کاراکتر مجاز است");

        context.SpecialOfferRequests.Add(new SpecialOfferRequest
        {
            SpecialOfferId = id,
            UserId = userId,
            Description = description,
            Status = SpecialOfferRequestStatus.Pending,
            RequesterLabCodeNew = user.GetLabCodeNew()!.Value,
            CreatedAt = DateTime.UtcNow,
        });

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return OperationResult.Failure("خطا در ثبت درخواست");
        }

        var adminUserId = await context.Users
            .AsNoTracking()
            .Where(u => u.UserType == UserType.AdminLab
                && u.CenterProfile != null
                && u.CenterProfile.LabCodeNew == offer.LabCodeNew)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        if (adminUserId != Guid.Empty)
        {
            var userName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(userName))
                userName = user.MobileNumber;

            await notificationService.CreateAsync(
                adminUserId,
                "درخواست پیشنهاد ویژه",
                $"کاربر {userName} برای پیشنهاد «{offer.Title}» درخواست ثبت کرد.",
                NotificationType.General);
        }

        return OperationResult.SuccessResult();
    }

    private static bool IsOwnLabOffer(User? user, int offerLabCodeNew)
    {
        if (user?.GetLabCodeNew() is not int labCodeNew)
            return false;

        return labCodeNew == offerLabCodeNew;
    }

    private static bool CanUserSubmitRequest(User? user, int offerLabCodeNew, bool hasSubmitted)
    {
        if (hasSubmitted || user == null)
            return false;

        if (user.UserType != UserType.UserLab && user.UserType != UserType.AdminLab)
            return false;

        if (!user.GetLabCodeNew().HasValue)
            return false;

        if (IsOwnLabOffer(user, offerLabCodeNew))
            return false;

        return true;
    }

    private sealed record LabSummary(string Name, Guid? ProfileId, bool HasLogo, string FirstName);

    private async Task<Dictionary<int, LabSummary>> ResolveLabSummariesAsync(List<int> labCodeNews)
    {
        if (labCodeNews.Count == 0)
            return new Dictionary<int, LabSummary>();

        var rows = await (
            from profile in context.CenterProfiles.AsNoTracking()
            where profile.CenterType == CenterType.Lab
                && profile.LabCodeNew.HasValue
                && labCodeNews.Contains(profile.LabCodeNew.Value)
            join admin in context.Users.AsNoTracking()
                on profile.Id equals admin.CenterProfileId into admins
            from admin in admins.Where(u => u.UserType == UserType.AdminLab).DefaultIfEmpty()
            select new
            {
                LabCodeNew = profile.LabCodeNew!.Value,
                profile.Id,
                profile.Name,
                profile.LogoPath,
                FirstName = admin != null ? admin.FirstName : string.Empty,
            }).ToListAsync();

        var result = new Dictionary<int, LabSummary>();
        foreach (var row in rows)
        {
            if (result.ContainsKey(row.LabCodeNew))
                continue;

            result[row.LabCodeNew] = new LabSummary(
                row.Name,
                row.Id,
                !string.IsNullOrWhiteSpace(row.LogoPath),
                row.FirstName);
        }

        return result;
    }
}
