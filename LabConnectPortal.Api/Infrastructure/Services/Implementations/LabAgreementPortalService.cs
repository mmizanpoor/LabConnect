using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;
using LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class LabAgreementPortalService(
    LabConnectDbContext context,
    IUserRepository userRepository,
    INotificationService notificationService) : ILabAgreementPortalService
{
    private const int MaxAttachmentBytes = 5 * 1024 * 1024;

    public async Task<OperationResult<long>> CreateFromPortalAsync(Guid userId, CreateLabAgreementFromPortalCommand command)
    {
        var admin = await userRepository.GetWithRolesAsync(userId);
        if (admin == null ||
            admin.UserType is not (UserType.AdminLab or UserType.UserLab) ||
            !admin.GetLabCodeNew().HasValue)
            return OperationResult<long>.Failure("دسترسی مجاز نیست");

        if (string.IsNullOrWhiteSpace(command.ContractNumber))
            return OperationResult<long>.Failure("شماره قرارداد الزامی است");

        if (string.IsNullOrWhiteSpace(command.Title))
            return OperationResult<long>.Failure("عنوان قرارداد الزامی است");

        if (string.IsNullOrWhiteSpace(command.Text))
            return OperationResult<long>.Failure("متن قرارداد الزامی است");

        if (command.ExpDate.Date < command.StartDate.Date)
            return OperationResult<long>.Failure("تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد");

        SpecialOfferRequest? request = null;
        if (command.SpecialOfferRequestId.HasValue)
        {
            request = await context.SpecialOfferRequests
                .Include(r => r.User).ThenInclude(u => u.CenterProfile)
                .Include(r => r.SpecialOffer)
                .ThenInclude(o => o.Tests)
                .ThenInclude(t => t.TestInfo)
                .FirstOrDefaultAsync(r => r.Id == command.SpecialOfferRequestId.Value);

            if (request == null)
                return OperationResult<long>.Failure("درخواست یافت نشد");

            if (request.SpecialOffer.LabCodeNew != admin.GetLabCodeNew()!.Value)
                return OperationResult<long>.Failure("دسترسی مجاز نیست");

            if (request.Status != SpecialOfferRequestStatus.AwaitingContractCreation)
                return OperationResult<long>.Failure("وضعیت درخواست برای ایجاد قرارداد مناسب نیست");

            if (request.LabAgreementId.HasValue)
                return OperationResult<long>.Failure("قرارداد برای این درخواست قبلاً ایجاد شده است");
        }

        var (primaryLab, receiverLab, labCodesError) = ResolveAgreementLabCodes(command, request);
        if (labCodesError != null)
            return OperationResult<long>.Failure(labCodesError);

        if (primaryLab != admin.GetLabCodeNew()!.Value)
            return OperationResult<long>.Failure("آزمایشگاه مبدأ نامعتبر است");

        var attachmentValidation = ValidateAttachments(command.Attachments);
        if (attachmentValidation != null)
            return OperationResult<long>.Failure(attachmentValidation);

        var testPrices = await BuildTestPricesAsync(command, request);
        if (testPrices.Error != null)
            return OperationResult<long>.Failure(testPrices.Error);

        var addendumContext = await ResolveAddendumContextAsync(command, primaryLab, receiverLab);
        if (addendumContext.Error != null)
            return OperationResult<long>.Failure(addendumContext.Error);

        var agreement = new LabAgreement
        {
            StartDate = command.StartDate.Date,
            ExpDate = addendumContext.ExpDate,
            ContractNumber = addendumContext.ContractNumber,
            PrimaryAgreementLabCodeNew = primaryLab,
            ReceiverAgreementLabCodeNew = receiverLab,
            Title = command.Title.Trim(),
            Text = command.Text,
            LaboratoryAgreementState = 0,
            ParentId = addendumContext.ParentId,
            Attachments = command.Attachments.Select((a, i) => new LabAgreementAttachment
            {
                FileName = a.FileName.Trim(),
                Remark = !string.IsNullOrWhiteSpace(a.Remark) ? a.Remark.Trim() : $"attach{i + 1}",
                ContentType = a.ContentType.Trim(),
            }).ToList(),
            TestPrices = testPrices.Items!,
        };

        context.LabAgreements.Add(agreement);

        try
        {
            await context.SaveChangesAsync();

            context.LabAgreementStatusHistories.Add(new LabAgreementStatusHistory
            {
                LabAgreementId = agreement.Id,
                UserType = (int)AgreementPartyType.Primary,
                PartyActionType = (int)PartyActionType.Submitted,
                ActionUserName = $"{admin.FirstName} {admin.LastName}".Trim(),
                ActionDateTime = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return OperationResult<long>.Failure("خطا در ذخیره قرارداد");
        }

        if (request != null)
        {
            request.LabAgreementId = agreement.Id;
            request.Status = SpecialOfferRequestStatus.ContractCreated;
            request.ReviewedAt = DateTime.UtcNow.ToLocalTime();
            request.ReviewedByUserId = userId;
            await context.SaveChangesAsync();

            await notificationService.CreateAsync(
                request.UserId,
                "قرارداد پیشنهاد ویژه",
                $"قرارداد برای درخواست پیشنهاد «{request.SpecialOffer.Title}» ایجاد شد.",
                NotificationType.General);
        }

        return OperationResult<long>.Success(agreement.Id);
    }

    private static (int PrimaryLab, int ReceiverLab, string? Error) ResolveAgreementLabCodes(
        CreateLabAgreementFromPortalCommand command,
        SpecialOfferRequest? request)
    {
        if (request != null)
        {
            var primaryLab = request.SpecialOffer.LabCodeNew;
            var receiverLab = ResolveRequesterLabCodeNew(request);

            if (primaryLab <= 0)
                return (0, 0, "کد آزمایشگاه پیشنهاددهنده نامعتبر است");

            if (receiverLab <= 0)
                return (0, 0, "کد آزمایشگاه درخواست‌دهنده نامعتبر است");

            if (primaryLab == receiverLab)
                return (0, 0, "آزمایشگاه پیشنهاددهنده و درخواست‌دهنده نمی‌توانند یکسان باشند");

            return (primaryLab, receiverLab, null);
        }

        if (command.PrimaryAgreementLabCodeNew <= 0 || command.ReceiverAgreementLabCodeNew <= 0)
            return (0, 0, "کد آزمایشگاه مبدأ و مقصد الزامی است");

        return (command.PrimaryAgreementLabCodeNew, command.ReceiverAgreementLabCodeNew, null);
    }

    private async Task<(long? ParentId, string ContractNumber, DateTime ExpDate, string? Error)> ResolveAddendumContextAsync(
        CreateLabAgreementFromPortalCommand command,
        int primaryLab,
        int receiverLab)
    {
        var contractNumber = command.ContractNumber.Trim();
        var expDate = command.ExpDate.Date;

        LabAgreement? parent = null;
        if (command.ParentId.HasValue)
        {
            parent = await context.LabAgreements
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.Id == command.ParentId.Value &&
                    !a.ParentId.HasValue &&
                    a.PrimaryAgreementLabCodeNew == primaryLab &&
                    a.ReceiverAgreementLabCodeNew == receiverLab);
        }

        parent ??= await FindActiveParentAgreementAsync(primaryLab, receiverLab);

        if (parent == null)
        {
            if (command.IsAddendum)
                return (null, contractNumber, expDate, "قرارداد فعال یافت نشد");

            return (null, contractNumber, expDate, null);
        }

        var today = DateTime.Now.Date;
        if (parent.StartDate.Date > today || parent.ExpDate.Date < today)
        {
            if (command.IsAddendum)
                return (null, contractNumber, expDate, "قرارداد فعال برای تاریخ جاری یافت نشد");

            return (null, contractNumber, expDate, null);
        }

        return (parent.Id, parent.ContractNumber.Trim(), parent.ExpDate.Date, null);
    }

    private Task<LabAgreement?> FindActiveParentAgreementAsync(int primaryLab, int receiverLab)
    {
        var now = DateTime.Now;
        return context.LabAgreements
            .AsNoTracking()
            .Where(x =>
                x.PrimaryAgreementLabCodeNew == primaryLab &&
                x.ReceiverAgreementLabCodeNew == receiverLab &&
                !x.ParentId.HasValue &&
                x.StartDate.Date <= now.Date &&
                x.ExpDate.Date >= now.Date &&
                !string.IsNullOrEmpty(x.PrimaryAgreementSign) &&
                !string.IsNullOrEmpty(x.ReceiverAgreementSign))
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync();
    }

    private static int ResolveRequesterLabCodeNew(SpecialOfferRequest request)
    {
        if (request.RequesterLabCodeNew > 0)
            return request.RequesterLabCodeNew;

        return request.User?.GetLabCodeNew() ?? 0;
    }

    private static string? ValidateAttachments(List<PortalLabAgreementAttachmentCommand> attachments)
    {
        foreach (var attachment in attachments)
        {
            if (string.IsNullOrWhiteSpace(attachment.Remark))
                return "نام فایل پیوست الزامی است";

            if (string.IsNullOrWhiteSpace(attachment.FileName))
                return "محتوای فایل پیوست الزامی است";

            try
            {
                var bytes = Convert.FromBase64String(attachment.FileName);
                if (bytes.Length > MaxAttachmentBytes)
                    return "حجم هر پیوست حداکثر ۵ مگابایت است";
            }
            catch
            {
                return "فرمت base64 فایل پیوست نامعتبر است";
            }
        }

        return null;
    }

    private async Task<(List<LabAgreementTestPrice>? Items, string? Error)> BuildTestPricesAsync(
        CreateLabAgreementFromPortalCommand command,
        SpecialOfferRequest? request)
    {
        if (command.TestPrices == null || command.TestPrices.Count == 0)
            return (null, "حداقل یک آزمایش باید انتخاب شود");

        var testInfoIds = command.TestPrices.Select(t => t.TestInfoId).Distinct().ToList();

        if (request != null)
        {
            var offerTestMap = request.SpecialOffer.Tests.ToDictionary(t => t.TestInfoId);
            var invalid = testInfoIds.Any(id => !offerTestMap.ContainsKey(id));
            if (invalid)
                return (null, "برخی آزمایشات انتخاب‌شده در پیشنهاد وجود ندارند");

            var items = testInfoIds.Select(id =>
            {
                var offerTest = offerTestMap[id];
                var testInfo = offerTest.TestInfo;
                var basePrice = testInfo.ApprovePrice ?? 0m;
                var approved = CalculateApprovedPrice(basePrice, offerTest.Discount);
                return new LabAgreementTestPrice
                {
                    TestId = testInfo.TestId,
                    TestName = testInfo.FullName ?? testInfo.ShortName ?? string.Empty,
                    BaseTariffApproved = basePrice,
                    Approved = approved,
                    CPNCode = testInfo.CPNCode,
                    NationalCode = testInfo.NationalCode,
                    FirstAdditions = 0,
                    SecondAdditions = 0,
                    UrgentAmount = 0,
                };
            }).ToList();

            return (items, null);
        }

        var testInfos = await context.TestInfos
            .AsNoTracking()
            .Where(t => testInfoIds.Contains(t.Id))
            .ToListAsync();

        if (testInfos.Count != testInfoIds.Count)
            return (null, "برخی آزمایشات انتخاب‌شده معتبر نیستند");

        var manualItems = testInfos.Select(t => new LabAgreementTestPrice
        {
            TestId = t.TestId,
            TestName = t.FullName ?? t.ShortName ?? string.Empty,
            BaseTariffApproved = t.ApprovePrice ?? 0m,
            Approved = t.ApprovePrice ?? 0m,
            CPNCode = t.CPNCode,
            NationalCode = t.NationalCode,
            FirstAdditions = 0,
            SecondAdditions = 0,
            UrgentAmount = 0,
        }).ToList();

        return (manualItems, null);
    }

    private static decimal CalculateApprovedPrice(decimal approvePrice, decimal? discount)
    {
        if (!discount.HasValue)
            return approvePrice;

        return Math.Round(approvePrice * (1 - discount.Value / 100m), 2);
    }
}
