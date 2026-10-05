using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Utils;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SpecialOfferService(
    LabConnectDbContext context,
    IUserRepository userRepository,
    INotificationService notificationService,
    ILabAgreementRepository labAgreementRepository,
    IOptions<PortalSettings> portalSettings) : ISpecialOfferService
{
    private readonly PortalSettings _portalSettings = portalSettings.Value;
    public async Task<OperationResult<List<SpecialOfferListItemDto>>> GetAllAsync(Guid userId)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<List<SpecialOfferListItemDto>>.Failure(access.Error);

        var today = DateTime.UtcNow.Date;
        var items = await context.SpecialOffers
            .AsNoTracking()
            .Where(o => o.LabCodeNew == access.LabCodeNew)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new SpecialOfferListItemDto
            {
                Id = o.Id,
                Title = o.Title,
                Summary = o.Summary,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                IsActive = o.IsActive,
                TestCount = o.Tests.Count,
                RequestCount = o.Requests.Count,
                IsExpired = o.EndDate.Date < today,
            })
            .ToListAsync();

        return OperationResult<List<SpecialOfferListItemDto>>.Success(items);
    }

    public async Task<OperationResult<SpecialOfferDetailDto>> GetByIdAsync(Guid userId, long id)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<SpecialOfferDetailDto>.Failure(access.Error);

        var offer = await context.SpecialOffers
            .AsNoTracking()
            .Include(o => o.Tests)
            .ThenInclude(t => t.TestInfo)
            .FirstOrDefaultAsync(o => o.Id == id && o.LabCodeNew == access.LabCodeNew);

        if (offer == null)
            return OperationResult<SpecialOfferDetailDto>.Failure("پیشنهاد یافت نشد");

        return OperationResult<SpecialOfferDetailDto>.Success(MapDetail(offer));
    }

    public async Task<OperationResult<SpecialOfferDetailDto>> CreateAsync(Guid userId, CreateSpecialOfferCommand command)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<SpecialOfferDetailDto>.Failure(access.Error);

        var validation = ValidateCommand(command);
        if (validation != null)
            return OperationResult<SpecialOfferDetailDto>.Failure(validation);

        var testsResult = await BuildTestsAsync(access.LabCodeNew!.Value, command.Tests);
        if (testsResult.Error != null)
            return OperationResult<SpecialOfferDetailDto>.Failure(testsResult.Error);

        var now = DateTime.UtcNow.ToLocalTime();
        var entity = new SpecialOffer
        {
            LabCodeNew = access.LabCodeNew!.Value,
            Title = command.Title.Trim(),
            Summary = command.Summary.Trim(),
            FullBody = command.FullBody ?? string.Empty,
            StartDate = command.StartDate.Date,
            EndDate = command.EndDate.Date,
            IsActive = command.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            Tests = testsResult.Tests!,
        };

        context.SpecialOffers.Add(entity);
        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SpecialOfferDetailDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetByIdAsync(userId, entity.Id);
    }

    public async Task<OperationResult<SpecialOfferDetailDto>> UpdateAsync(Guid userId, UpdateSpecialOfferCommand command)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<SpecialOfferDetailDto>.Failure(access.Error);

        var validation = ValidateCommand(command);
        if (validation != null)
            return OperationResult<SpecialOfferDetailDto>.Failure(validation);

        var entity = await context.SpecialOffers
            .Include(o => o.Tests)
            .FirstOrDefaultAsync(o => o.Id == command.Id && o.LabCodeNew == access.LabCodeNew);

        if (entity == null)
            return OperationResult<SpecialOfferDetailDto>.Failure("پیشنهاد یافت نشد");

        var testsResult = await BuildTestsAsync(access.LabCodeNew!.Value, command.Tests);
        if (testsResult.Error != null)
            return OperationResult<SpecialOfferDetailDto>.Failure(testsResult.Error);

        entity.Title = command.Title.Trim();
        entity.Summary = command.Summary.Trim();
        entity.FullBody = command.FullBody ?? string.Empty;
        entity.StartDate = command.StartDate.Date;
        entity.EndDate = command.EndDate.Date;
        entity.IsActive = command.IsActive;
        entity.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        context.SpecialOfferTests.RemoveRange(entity.Tests);
        entity.Tests = testsResult.Tests!;

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<SpecialOfferDetailDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        return await GetByIdAsync(userId, entity.Id);
    }

    public async Task<OperationResult> DeleteAsync(Guid userId, long id)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var entity = await context.SpecialOffers
            .FirstOrDefaultAsync(o => o.Id == id && o.LabCodeNew == access.LabCodeNew);

        if (entity == null)
            return OperationResult.Failure("پیشنهاد یافت نشد");

        context.SpecialOffers.Remove(entity);
        return await SaveChangesAsync();
    }

    public async Task<OperationResult<List<SpecialOfferRequestDto>>> GetRequestsAsync(Guid userId, long specialOfferId)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<List<SpecialOfferRequestDto>>.Failure(access.Error);

        var exists = await context.SpecialOffers
            .AsNoTracking()
            .AnyAsync(o => o.Id == specialOfferId && o.LabCodeNew == access.LabCodeNew);

        if (!exists)
            return OperationResult<List<SpecialOfferRequestDto>>.Failure("پیشنهاد یافت نشد");

        var items = await context.SpecialOfferRequests
            .AsNoTracking()
            .Include(r => r.User).ThenInclude(u => u.CenterProfile)
            .Where(r => r.SpecialOfferId == specialOfferId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var labCodeNews = items
            .Select(r => ResolveRequesterLabCodeNew(r))
            .Where(c => c > 0)
            .Distinct()
            .ToList();
        var labNames = await ResolveLabNamesAsync(labCodeNews);

        var dtos = items.Select(r =>
        {
            var requesterLabCode = ResolveRequesterLabCodeNew(r);
            return new SpecialOfferRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserDisplayName = ($"{r.User.FirstName} {r.User.LastName}").Trim(),
                MobileNumber = r.User.MobileNumber,
                Description = r.Description,
                Status = (int)r.Status,
                RejectionReason = r.RejectionReason,
                RequesterLabCodeNew = requesterLabCode,
                RequesterLabName = labNames.GetValueOrDefault(requesterLabCode, string.Empty),
                LabAgreementId = r.LabAgreementId,
                CreatedAt = r.CreatedAt,
                ReviewedAt = r.ReviewedAt,
            };
        }).ToList();

        return OperationResult<List<SpecialOfferRequestDto>>.Success(dtos);
    }

    public async Task<OperationResult> ApproveRequestAsync(Guid userId, long requestId)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var request = await context.SpecialOfferRequests
            .Include(r => r.SpecialOffer)
            .Include(r => r.User).ThenInclude(u => u.CenterProfile)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.SpecialOffer.LabCodeNew == access.LabCodeNew);

        if (request == null)
            return OperationResult.Failure("درخواست یافت نشد");

        if (request.Status != SpecialOfferRequestStatus.Pending)
            return OperationResult.Failure("فقط درخواست‌های در انتظار بررسی قابل تأیید هستند");

        request.Status = SpecialOfferRequestStatus.AwaitingContractCreation;
        request.ReviewedAt = DateTime.UtcNow.ToLocalTime();
        request.ReviewedByUserId = userId;
        request.RejectionReason = null;

        await context.SaveChangesAsync();

        await notificationService.CreateAsync(
            request.UserId,
            "تأیید درخواست پیشنهاد ویژه",
            $"درخواست شما برای پیشنهاد «{request.SpecialOffer.Title}» تأیید شد و در انتظار ایجاد قرارداد است.",
            NotificationType.General);

        await notificationService.SendNotificationAsync(new NotificationCommandBase
        {
            Title = "پیشنهاد ویژه جدید",
            Message = $"«{request.SpecialOffer.Title}» در سامانه منتشر شد.",
            ExpireDate = DateTime.UtcNow.ToLocalTime().AddDays(3),
            IsActive = true,
            TargetUsers = [],
            NotificationActions =
            [
                new NotificationAction
                {
                    NotificationActionType = NotificationActionType.SystemEntity,
                    Label = "مشاهده پیشنهاد",
                    Color = "bg-primary-600 text-white",
                    SystemEntityId = new Guid("7f3e21a8-4b9c-4d6e-9a1f-2c8e5d4b6a70"),
                    Url = _portalSettings.BuildPublicUrl($"special-offers/{request.SpecialOffer.Id}"),
                    OpenTab = true,
                },
            ],
        });

        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> RejectRequestAsync(Guid userId, RejectSpecialOfferRequestCommand command)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult.Failure(access.Error);

        var reason = command.Reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(reason))
            return OperationResult.Failure("دلیل رد الزامی است");

        var request = await context.SpecialOfferRequests
            .Include(r => r.SpecialOffer)
            .Include(r => r.User).ThenInclude(u => u.CenterProfile)
            .FirstOrDefaultAsync(r => r.Id == command.RequestId && r.SpecialOffer.LabCodeNew == access.LabCodeNew);

        if (request == null)
            return OperationResult.Failure("درخواست یافت نشد");

        if (request.Status != SpecialOfferRequestStatus.Pending)
            return OperationResult.Failure("فقط درخواست‌های در انتظار بررسی قابل رد هستند");

        request.Status = SpecialOfferRequestStatus.Rejected;
        request.RejectionReason = reason;
        request.ReviewedAt = DateTime.UtcNow.ToLocalTime();
        request.ReviewedByUserId = userId;

        await context.SaveChangesAsync();

        await notificationService.CreateAsync(
            request.UserId,
            "رد درخواست پیشنهاد ویژه",
            $"درخواست پیشنهاد «{request.SpecialOffer.Title}» رد شد. دلیل: {reason}",
            NotificationType.General);

        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult<SpecialOfferRequestForAgreementDto>> GetRequestForAgreementAsync(Guid userId, long requestId)
    {
        var access = await GetAdminLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<SpecialOfferRequestForAgreementDto>.Failure(access.Error);

        var request = await context.SpecialOfferRequests
            .AsNoTracking()
            .Include(r => r.User).ThenInclude(u => u.CenterProfile)
            .Include(r => r.SpecialOffer)
            .ThenInclude(o => o.Tests)
            .ThenInclude(t => t.TestInfo)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.SpecialOffer.LabCodeNew == access.LabCodeNew);

        if (request == null)
            return OperationResult<SpecialOfferRequestForAgreementDto>.Failure("درخواست یافت نشد");

        if (request.Status != SpecialOfferRequestStatus.AwaitingContractCreation)
            return OperationResult<SpecialOfferRequestForAgreementDto>.Failure("وضعیت درخواست برای ایجاد قرارداد مناسب نیست");

        var primaryLabCodeNew = request.SpecialOffer.LabCodeNew;
        var receiverLabCodeNew = request.RequesterLabCodeNew > 0
            ? request.RequesterLabCodeNew
            : request.User?.GetLabCodeNew() ?? 0;

        var primaryLabNames = await ResolveLabNamesAsync([primaryLabCodeNew]);
        var receiverLabNames = await ResolveLabNamesAsync([receiverLabCodeNew]);

        var activeAgreement = primaryLabCodeNew > 0 && receiverLabCodeNew > 0
            ? await labAgreementRepository.GetLabAgreementByLabCode(primaryLabCodeNew, receiverLabCodeNew)
            : null;

        return OperationResult<SpecialOfferRequestForAgreementDto>.Success(new SpecialOfferRequestForAgreementDto
        {
            RequestId = request.Id,
            SpecialOfferId = request.SpecialOfferId,
            OfferTitle = request.SpecialOffer.Title,
            OfferStartDate = request.SpecialOffer.StartDate,
            OfferEndDate = request.SpecialOffer.EndDate,
            Description = request.Description,
            PrimaryLabCodeNew = primaryLabCodeNew,
            PrimaryLabName = primaryLabNames.GetValueOrDefault(primaryLabCodeNew, string.Empty),
            ReceiverLabCodeNew = receiverLabCodeNew,
            ReceiverLabName = receiverLabNames.GetValueOrDefault(receiverLabCodeNew, string.Empty),
            IsAddendum = activeAgreement?.Id != null,
            ParentId = activeAgreement?.Id,
            ActiveAgreementContractNumber = activeAgreement?.ContractNumber,
            ActiveAgreementExpDate = activeAgreement?.ExpDate,
            Tests = request.SpecialOffer.Tests
                .OrderBy(t => t.TestInfo.FullName)
                .Select(t => new SpecialOfferTestItemDto
                {
                    TestInfoId = t.TestInfoId,
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

    public async Task<OperationResult<SpecialOfferDashboardStatsDto>> GetAdminDashboardStatsAsync()
    {
        var (fromShamsi, toShamsi) = CustomConverter.GetShamsiMonthRange(DateTime.Now);
        var from = fromShamsi.GetShamsiDatePart().ShamsiStrToMiladiDate();
        var to = toShamsi.GetShamsiDatePart().ShamsiStrToMiladiDate()
            .Date.AddHours(23).AddMinutes(59).AddSeconds(59);

        var grouped = await context.SpecialOffers
            .AsNoTracking()
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .GroupBy(o => o.LabCodeNew)
            .Select(g => new { LabCodeNew = g.Key, OfferCount = g.Count() })
            .OrderByDescending(x => x.OfferCount)
            .ThenBy(x => x.LabCodeNew)
            .ToListAsync();

        var labCodeNews = grouped.Select(x => x.LabCodeNew).ToList();
        var labNames = await ResolveLabNamesAsync(labCodeNews);

        var rows = grouped.Select(x => new SpecialOfferLabMonthlyRowDto
        {
            LabCodeNew = x.LabCodeNew,
            LabName = labNames.GetValueOrDefault(x.LabCodeNew, string.Empty),
            OfferCount = x.OfferCount,
        }).ToList();

        return OperationResult<SpecialOfferDashboardStatsDto>.Success(new SpecialOfferDashboardStatsDto
        {
            MonthlyOffersByLab = rows,
        });
    }

    public async Task<OperationResult<List<AdminSpecialOfferListItemDto>>> GetAllForAdminAsync()
    {
        var today = DateTime.UtcNow.Date;
        var offers = await context.SpecialOffers
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new AdminSpecialOfferListItemDto
            {
                Id = o.Id,
                Title = o.Title,
                Summary = o.Summary,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                IsActive = o.IsActive,
                IsExpired = o.EndDate.Date < today,
                LabCodeNew = o.LabCodeNew,
                TestCount = o.Tests.Count,
                RequestCount = o.Requests.Count,
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync();

        var labNames = await ResolveLabNamesAsync(offers.Select(o => o.LabCodeNew).Distinct().ToList());

        var offerIds = offers.Select(o => o.Id).ToList();
        var requests = await context.SpecialOfferRequests
            .AsNoTracking()
            .Include(r => r.User).ThenInclude(u => u.CenterProfile)
            .Where(r => offerIds.Contains(r.SpecialOfferId))
            .ToListAsync();

        var requesterCodesByOffer = requests
            .GroupBy(r => r.SpecialOfferId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(ResolveRequesterLabCodeNew).Where(c => c > 0).Distinct().ToList());

        var allRequesterCodes = requesterCodesByOffer.Values
            .SelectMany(codes => codes)
            .Distinct()
            .ToList();
        var requesterLabNames = await ResolveLabNamesAsync(allRequesterCodes);

        foreach (var offer in offers)
        {
            offer.LabName = labNames.GetValueOrDefault(offer.LabCodeNew, string.Empty);
            if (!requesterCodesByOffer.TryGetValue(offer.Id, out var codes) || codes.Count == 0)
            {
                offer.RequesterLabNames = string.Empty;
                continue;
            }

            offer.RequesterLabNames = string.Join("، ", codes
                .Select(c => requesterLabNames.GetValueOrDefault(c, string.Empty))
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct());
        }

        return OperationResult<List<AdminSpecialOfferListItemDto>>.Success(offers);
    }

    public async Task<OperationResult<AdminSpecialOfferDetailDto>> GetByIdForAdminAsync(long id)
    {
        var today = DateTime.UtcNow.Date;
        var offer = await context.SpecialOffers
            .AsNoTracking()
            .Include(o => o.Tests)
            .ThenInclude(t => t.TestInfo)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
            return OperationResult<AdminSpecialOfferDetailDto>.Failure("پیشنهاد یافت نشد");

        var labNames = await ResolveLabNamesAsync([offer.LabCodeNew]);
        var requestCount = await context.SpecialOfferRequests.AsNoTracking().CountAsync(r => r.SpecialOfferId == id);
        var detail = MapDetail(offer);

        return OperationResult<AdminSpecialOfferDetailDto>.Success(new AdminSpecialOfferDetailDto
        {
            Id = detail.Id,
            Title = detail.Title,
            Summary = detail.Summary,
            FullBody = detail.FullBody,
            StartDate = detail.StartDate,
            EndDate = detail.EndDate,
            IsActive = detail.IsActive,
            Tests = detail.Tests,
            LabCodeNew = offer.LabCodeNew,
            LabName = labNames.GetValueOrDefault(offer.LabCodeNew, string.Empty),
            IsExpired = offer.EndDate.Date < today,
            RequestCount = requestCount,
            CreatedAt = offer.CreatedAt,
        });
    }

    private async Task<Dictionary<int, string>> ResolveLabNamesAsync(List<int> labCodeNews)
    {
        if (labCodeNews.Count == 0)
            return new Dictionary<int, string>();

        var centerProfiles = await context.CenterProfiles
            .AsNoTracking()
            .Where(c => c.CenterType == CenterType.Lab
                && c.LabCodeNew.HasValue
                && labCodeNews.Contains(c.LabCodeNew.Value))
            .Select(c => new { LabCodeNew = c.LabCodeNew!.Value, c.Name })
            .ToListAsync();

        var result = new Dictionary<int, string>();
        foreach (var code in labCodeNews.Distinct())
        {
            result[code] = centerProfiles.FirstOrDefault(c => c.LabCodeNew == code)?.Name
                ?? string.Empty;
        }

        return result;
    }

    private static int ResolveRequesterLabCodeNew(SpecialOfferRequest request)
    {
        if (request.RequesterLabCodeNew > 0)
            return request.RequesterLabCodeNew;

        return request.User.GetLabCodeNew() ?? 0;
    }

    private static SpecialOfferDetailDto MapDetail(SpecialOffer offer) => new()
    {
        Id = offer.Id,
        Title = offer.Title,
        Summary = offer.Summary,
        FullBody = offer.FullBody,
        StartDate = offer.StartDate,
        EndDate = offer.EndDate,
        IsActive = offer.IsActive,
        Tests = offer.Tests
            .OrderBy(t => t.TestInfo.FullName)
            .Select(t => new SpecialOfferTestItemDto
            {
                TestInfoId = t.TestInfoId,
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
    };

    private static string? ValidateCommand(CreateSpecialOfferCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return "عنوان الزامی است";

        if (string.IsNullOrWhiteSpace(command.Summary))
            return "متن خلاصه الزامی است";

        if (command.EndDate.Date < command.StartDate.Date)
            return "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد";

        if (command.Tests == null || command.Tests.Count == 0)
            return "حداقل یک آزمایش باید انتخاب شود";

        return null;
    }

    private async Task<(List<SpecialOfferTest>? Tests, string? Error)> BuildTestsAsync(
        int labCodeNew,
        List<SaveSpecialOfferTestCommand> tests)
    {
        if (tests == null || tests.Count == 0)
            return (null, "حداقل یک آزمایش باید انتخاب شود");

        var testInfoIds = tests.Select(t => t.TestInfoId).Distinct().ToList();
        var validIds = await context.TestInfos
            .AsNoTracking()
            .Where(t => t.LabCodeNew == labCodeNew && testInfoIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        if (validIds.Count != testInfoIds.Count)
            return (null, "برخی آزمایشات انتخاب‌شده معتبر نیستند");

        var entities = tests.Select(t => new SpecialOfferTest
        {
            TestInfoId = t.TestInfoId,
            Discount = t.Discount,
            MaxSamples = t.MaxSamples,
        }).ToList();

        return (entities, null);
    }

    private async Task<(int? LabCodeNew, string? Error)> GetAdminLabCodeNewAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || user.UserType is not (UserType.AdminLab or UserType.UserLab))
            return (null, "دسترسی مجاز نیست");

        if (!user.GetLabCodeNew().HasValue)
            return (null, "کد آزمایشگاه تعریف نشده است");

        return (user.GetLabCodeNew()!.Value, null);
    }

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (DbUpdateException)
        {
            return OperationResult.Failure("خطا در ذخیره‌سازی");
        }
    }
}
